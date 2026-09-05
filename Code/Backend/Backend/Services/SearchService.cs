using System.Linq.Expressions;
using Backend.Data;
using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Backend.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class SearchService : ISearchService
{

    private readonly AppDbContext _db;
    private readonly ITermExtractionService _termExtraction;
    private readonly TermGraph _termGraph;
    private readonly IProductViewRepository _productViewRepo;
    private readonly ILogger<SearchService> _logger;
    private readonly SearchResultCache _cache;

    public SearchService(
        AppDbContext db,
        ITermExtractionService termExtraction,
        TermGraph termGraph,
        IProductViewRepository productViewRepo,
        ILogger<SearchService> logger,
        SearchResultCache cache)
    {
        _db = db;
        _termExtraction = termExtraction;
        _termGraph = termGraph;
        _productViewRepo = productViewRepo;
        _logger = logger;
        _cache = cache;
    }

    public async Task<SearchResultDto> SearchProductAsync(SearchRequestDto request)
    {
        request.Page = Math.Max(1, request.Page);
        request.PageSize = request.PageSize < 1 ? 20 : Math.Min(50, request.PageSize);
        if ((long)(request.Page - 1) * request.PageSize > int.MaxValue)
            throw new ArgumentException("页码超出支持范围");

        var cached = _cache.TryGet(request);
        if (cached != null) return cached;
        if (string.IsNullOrWhiteSpace(request.Keyword))
            throw new ArgumentException("搜索缓存已过期、无效或与查询条件不匹配，请携带 keyword 重新搜索");

        // 分词
        var rawTerms = _termExtraction.Extract(request.Keyword);
        if (rawTerms.Count == 0)
        {
            return NewEmptyResult(request);
        }

        // 查询扩展
        List<(string term, double weight)> expandedTerms;
        if (_termGraph.IsInitialized)
            expandedTerms = _termGraph.ExpandQuery(rawTerms);
        else
        {
            expandedTerms = rawTerms.Select(t => (t, 1.0)).ToList();
            _logger.LogWarning("TermGraph not initialized, using raw terms only");
        }

        var termList = expandedTerms.Select(t => t.term).ToList();
        var filter = BuildKeywordFilter(termList);

        // 执行搜索
        if (string.IsNullOrEmpty(request.SortBy) || request.SortBy == "relevance")
        {
            return await SearchWithRelevanceSort(filter, expandedTerms, termList, request);
        }
        else
        {
            return await SearchWithDbSort(filter, termList, request);
        }
    }

    public async Task NotifyProductCreatedAsync(long productId)
    {
        try
        {
            await _termGraph.ProcessAndSaveProductAsync(productId, _termExtraction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update TermGraph for product {ProductId}; rebuild required", productId);
        }
    }

    public async Task RebuildGraphAsync()
    {
        _logger.LogInformation("Starting full TermGraph rebuild...");

        await _termGraph.RebuildAsync(_termExtraction);

        _logger.LogInformation("Full TermGraph rebuild completed: {Nodes} nodes, {Edges} edges",
            _termGraph.NodeCount, _termGraph.EdgeCount);
    }

    /// <summary>
    /// 相关性排序
    /// </summary>
    private async Task<SearchResultDto> SearchWithRelevanceSort(
        Expression<Func<Product, bool>>? filter,
        List<(string term, double weight)> expandedTerms,
        List<string> termList,
        SearchRequestDto request)
    {

        var baseQuery = _db.Products
            .Where(p => p.Status == ProductStatus.Available);

        if (request.UserId.HasValue)
            baseQuery = baseQuery.Where(p => p.UserId == request.UserId.Value);

        if (filter != null)
            baseQuery = baseQuery.Where(filter);

        var products = await baseQuery
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

        var productIds = products.Select(p => p.ProductId).ToList();
        var viewCounts = await _productViewRepo.GetViewCountsAsync(productIds);

        var allItems = products
            .Select(p => new
            {
                Card = ProductService.ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)),
                Score = ComputeRelevanceScore(p, expandedTerms)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Card.ProductId)
            .Select(x => x.Card)
            .ToArray();

        // 缓存排序结果
        var searchId = _cache.Store(request, allItems, termList);

        var pageItems = allItems
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new SearchResultDto
        {
            SearchId = searchId,
            Items = pageItems,
            TotalCount = allItems.Length,
            Page = request.Page,
            PageSize = request.PageSize,
            ExpandedTerms = termList
        };

    }

    /// <summary>
    /// 数据库负责排序和分页，不缓存
    /// </summary>
    private async Task<SearchResultDto> SearchWithDbSort(
        Expression<Func<Product, bool>>? filter,
        List<string> termList,
        SearchRequestDto request)
    {
        var baseQuery = _db.Products
            .Where(p => p.Status == ProductStatus.Available);

        if (request.UserId.HasValue)
            baseQuery = baseQuery.Where(p => p.UserId == request.UserId.Value);

        if (filter != null)
            baseQuery = baseQuery.Where(filter);

        var orderedQuery = ApplySorting(baseQuery, request.SortBy!);

        var totalCount = await orderedQuery.CountAsync();
        var products = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

        var productIds = products.Select(p => p.ProductId).ToList();
        var viewCounts = await _productViewRepo.GetViewCountsAsync(productIds);

        var items = products
            .Select(p => ProductService.ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();

        return new SearchResultDto
        {
            SearchId = "",
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            ExpandedTerms = termList
        };
    }

    private SearchResultDto NewEmptyResult(SearchRequestDto request) => new()
    {
        SearchId = string.IsNullOrEmpty(request.SortBy) || request.SortBy == "relevance"
            ? _cache.Store(request, [], []) : "",
        Items = new(),
        TotalCount = 0,
        Page = request.Page,
        PageSize = request.PageSize
    };

    /// <summary>
    /// 构造关键词
    /// </summary>
    private static Expression<Func<Product, bool>>? BuildKeywordFilter(List<string> terms)
    {
        if (terms.Count == 0) return null;

        var parameter = Expression.Parameter(typeof(Product), "p");
        var nameProp = Expression.Property(parameter, nameof(Product.Name));
        var infoProp = Expression.Property(parameter, nameof(Product.Info));
        var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
        var nullConst = Expression.Constant(null, typeof(string));

        Expression? body = null;

        foreach (var term in terms)
        {
            var termConst = Expression.Constant(term, typeof(string));

            var nameContains = Expression.Call(nameProp, containsMethod, termConst);

            var infoNotNull = Expression.NotEqual(infoProp, nullConst);
            var infoContains = Expression.Call(infoProp, containsMethod, termConst);
            var infoMatch = Expression.AndAlso(infoNotNull, infoContains);

            var termMatch = Expression.OrElse(nameContains, infoMatch);

            body = body == null ? termMatch : Expression.OrElse(body, termMatch);
        }

        return body != null
            ? Expression.Lambda<Func<Product, bool>>(body, parameter)
            : null;
    }

    private static IOrderedQueryable<Product> ApplySorting(IQueryable<Product> query, string sortBy) => sortBy switch
    {
        "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.ProductId),
        "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.ProductId),
        _ => query.OrderByDescending(p => p.ReleaseDate).ThenBy(p => p.ProductId), // latest
    };

    /// <summary>
    /// 相关性分数：扩展词条加权求和。Name 命中权重 2.0，Info 命中权重 1.0。
    /// </summary>
    private static int ComputeRelevanceScore(Product product, List<(string term, double weight)> expandedTerms)
    {
        double score = 0;
        var name = product.Name ?? "";
        var info = product.Info ?? "";

        foreach (var (term, weight) in expandedTerms)
        {
            if (name.Contains(term, StringComparison.OrdinalIgnoreCase))
                score += 2.0 * weight;
            if (info.Contains(term, StringComparison.OrdinalIgnoreCase))
                score += 1.0 * weight;
        }

        return (int)(score * 100);
    }
}
