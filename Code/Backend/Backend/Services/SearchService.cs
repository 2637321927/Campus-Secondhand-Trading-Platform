using System.Collections.Concurrent;
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

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<string, CachedSearch> _cache = new();

    private class CachedSearch
    {
        public ProductCardDto[] Items { get; init; } = [];
        public int TotalCount { get; init; }
        public List<string> ExpandedTerms { get; init; } = [];
        public DateTime CreatedAt { get; init; }
    }

    public SearchService(
        AppDbContext db,
        ITermExtractionService termExtraction,
        TermGraph termGraph,
        IProductViewRepository productViewRepo,
        ILogger<SearchService> logger)
    {
        _db = db;
        _termExtraction = termExtraction;
        _termGraph = termGraph;
        _productViewRepo = productViewRepo;
        _logger = logger;
    }

    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request)
    {
        // 翻页请求直接切片返回
        if (!string.IsNullOrEmpty(request.SearchId) &&
            _cache.TryGetValue(request.SearchId, out var cached))
        {
            if (DateTime.UtcNow - cached.CreatedAt > CacheTtl)
                _cache.TryRemove(request.SearchId, out _);
            else
                return SliceFromCache(cached, request);
        }

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
        if (!_termGraph.IsInitialized)
        {
            _logger.LogInformation("TermGraph not initialized, skipping product notification");
            return;
        }

        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProductId == productId);

        if (product == null) return;

        var text = $"{product.Name} {product.Info ?? ""}";
        var terms = _termExtraction.Extract(text);

        if (terms.Count == 0) return;

        _termGraph.ProcessNewProduct(terms, product.UserId, product.CategoryId);
        await _termGraph.SaveToDatabaseAsync();

        _logger.LogDebug("Processed new product {ProductId} with {TermCount} terms", productId, terms.Count);
    }

    public async Task RebuildGraphAsync()
    {
        _logger.LogInformation("Starting full TermGraph rebuild...");

        var products = await _db.Products
            .AsNoTracking()
            .Where(p => p.Status != ProductStatus.Removed)
            .Select(p => new { p.ProductId, p.Name, p.Info, p.UserId, p.CategoryId })
            .ToListAsync();

        foreach (var product in products)
        {
            var text = $"{product.Name} {product.Info ?? ""}";
            var terms = _termExtraction.Extract(text);

            if (terms.Count > 0)
                _termGraph.ProcessNewProduct(terms, product.UserId, product.CategoryId);
        }

        _logger.LogInformation("Full TermGraph rebuild completed: {Nodes} nodes, {Edges} edges",
            _termGraph.NodeCount, _termGraph.EdgeCount);

        await _termGraph.SaveToDatabaseAsync();
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
                Card = ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)),
                Score = ComputeRelevanceScore(p, expandedTerms)
            })
            .OrderByDescending(x => x.Score)
            .Select(x => x.Card)
            .ToArray();

        // 缓存全量排序结果
        PurgeExpiredCache();
        var searchId = Guid.NewGuid().ToString("N")[..12];
        _cache[searchId] = new CachedSearch
        {
            Items = allItems,
            TotalCount = allItems.Length,
            ExpandedTerms = termList,
            CreatedAt = DateTime.UtcNow
        };

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
            .Select(p => ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
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

    private SearchResultDto SliceFromCache(CachedSearch cached, SearchRequestDto request)
    {
        var items = cached.Items
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new SearchResultDto
        {
            SearchId = request.SearchId!,
            Items = items,
            TotalCount = cached.TotalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            ExpandedTerms = cached.ExpandedTerms
        };
    }

    private void PurgeExpiredCache()
    {
        var cutoff = DateTime.UtcNow - CacheTtl;
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryGetValue(key, out var entry) && entry.CreatedAt < cutoff)
                _cache.TryRemove(key, out _);
        }
    }

    private static SearchResultDto NewEmptyResult(SearchRequestDto request) => new()
    {
        SearchId = Guid.NewGuid().ToString("N")[..12],
        Items = new(),
        TotalCount = 0,
        Page = request.Page,
        PageSize = request.PageSize
    };

    /// <summary>
    /// 构造关键词OR过滤表达式。
    /// p => (p.Name.Contains(t1) || (p.Info != null && p.Info.Contains(t1)))
    ///    || (p.Name.Contains(t2) || (p.Info != null && p.Info.Contains(t2)))
    ///    || ...
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
        "price_asc" => query.OrderBy(p => p.Price),
        "price_desc" => query.OrderByDescending(p => p.Price),
        _ => query.OrderByDescending(p => p.ReleaseDate), // latest
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

    private static ProductCardDto ToProductCard(Product p, int viewCount) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        Price = p.Price,
        CoverImageUrl = p.Images?
            .OrderBy(i => i.ImgIndex)
            .FirstOrDefault()?.ImgFileId is long fileId
                ? $"/api/files/{fileId}" : null,
        SellerName = p.Seller?.UserName ?? "",
        ReleaseDate = p.ReleaseDate,
        ViewCount = viewCount
    };
}
