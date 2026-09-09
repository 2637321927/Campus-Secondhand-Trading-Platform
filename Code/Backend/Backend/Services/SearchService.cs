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
    private readonly ITermSimilarityStore _similarityStore;
    private readonly ITermSimilarityRefreshService _similarityRefresh;

    public SearchService(
        AppDbContext db,
        ITermExtractionService termExtraction,
        TermGraph termGraph,
        IProductViewRepository productViewRepo,
        ILogger<SearchService> logger,
        SearchResultCache cache,
        ITermSimilarityStore similarityStore,
        ITermSimilarityRefreshService similarityRefresh)
    {
        _db = db;
        _termExtraction = termExtraction;
        _termGraph = termGraph;
        _productViewRepo = productViewRepo;
        _logger = logger;
        _cache = cache;
        _similarityStore = similarityStore;
        _similarityRefresh = similarityRefresh;
    }

    public async Task<SearchResultDto> SearchProductAsync(SearchRequestDto request)
    {
        request.Page = Math.Max(1, request.Page);
        request.PageSize = request.PageSize < 1 ? 20 : Math.Min(50, request.PageSize);
        if ((long)(request.Page - 1) * request.PageSize > int.MaxValue)
            throw new ArgumentException("页码超出支持范围");

        // 用户主页内搜索
        if (request.UserId.HasValue)
            return await SearchWithinUserAsync(request);

        var cached = _cache.TryGet(request);
        if (cached != null)
            return await BuildCachedResultAsync(cached, request);
        if (string.IsNullOrWhiteSpace(request.Keyword))
            throw new ArgumentException("搜索缓存已过期、无效或与查询条件不匹配，请携带 keyword 重新搜索");

        // 分词
        var rawTerms = _termExtraction.Extract(request.Keyword);
        if (rawTerms.Count == 0)
        {
            return NewEmptyResult(request);
        }

        var originalTerms = rawTerms.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var similarTerms = MergeSimilarTerms(originalTerms, _similarityStore.Current);
        var displayTerms = originalTerms
            .Concat(similarTerms.Select(x => x.Term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var filter = BuildKeywordFilter(originalTerms);

        // 执行搜索
        if (string.IsNullOrEmpty(request.SortBy) || request.SortBy == "relevance")
        {
            return await SearchWithRelevanceSort(filter, originalTerms, similarTerms, displayTerms, request);
        }
        else
        {
            return await SearchWithDbSort(filter, displayTerms, request);
        }
    }

    /// <summary>
    /// 用户主页内搜索：对原始关键词整体模糊匹配（与收藏搜索一致），不走分词/词条/相似词扩展
    /// </summary>
    private async Task<SearchResultDto> SearchWithinUserAsync(SearchRequestDto request)
    {
        var keyword = (request.Keyword ?? "").Trim();
        if (keyword.Length == 0)
        {
            return new SearchResultDto
            {
                SearchId = "",
                Items = new(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        var baseQuery = _db.Products.AsNoTracking()
            .Where(p => p.UserId == request.UserId!.Value)
            .Where(p => p.Name.Contains(keyword)
                || (p.Info != null && p.Info.Contains(keyword)));

        var orderedQuery = ApplySorting(baseQuery, request.SortBy ?? "latest");

        var totalCount = await orderedQuery.CountAsync();
        var products = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

        var productIds = products.Select(p => p.ProductId).ToList();
        var viewCounts = await _productViewRepo.GetViewCountsAsync(productIds);

        return new SearchResultDto
        {
            SearchId = "",
            Items = products
                .Select(p => ProductService.ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
                .ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
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
        await _similarityRefresh.RefreshAsync();

        _logger.LogInformation("Full TermGraph rebuild completed: {Nodes} nodes, {Edges} edges",
            _termGraph.NodeCount, _termGraph.EdgeCount);
    }

    public Task RefreshSimilarityAsync(CancellationToken cancellationToken = default) =>
        _similarityRefresh.RefreshAsync(cancellationToken);

    /// <summary>
    /// 相关性排序
    /// </summary>
    private async Task<SearchResultDto> SearchWithRelevanceSort(
        Expression<Func<Product, bool>>? filter,
        List<string> originalTerms,
        List<WeightedTerm> similarTerms,
        List<string> displayTerms,
        SearchRequestDto request)
    {
        var baseQuery = _db.Products.AsNoTracking()
            .Where(p => p.Status == ProductStatus.Available);
        if (request.UserId.HasValue)
            baseQuery = baseQuery.Where(p => p.UserId == request.UserId.Value);
        if (request.CategoryId.HasValue)
            baseQuery = ApplyCategoryFilter(baseQuery, request.CategoryId.Value);
        if (filter != null)
            baseQuery = baseQuery.Where(filter);

        var candidates = await baseQuery
            .Select(p => new { p.ProductId, p.Name, p.Info })
            .ToListAsync();

        var orderedIds = candidates
            .Select(p => new
            {
                p.ProductId,
                OriginalTermCoverage = originalTerms.Count(term => Contains(p.Name, p.Info, term)),
                SimilarTermScore = ComputeSimilarTermScore(p.Name, p.Info, similarTerms)
            })
            .OrderByDescending(x => x.OriginalTermCoverage)
            .ThenByDescending(x => x.SimilarTermScore)
            .ThenBy(x => x.ProductId)
            .Select(x => x.ProductId)
            .ToArray();

        var searchId = _cache.Store(request, orderedIds, displayTerms);
        return await BuildResultFromIdsAsync(searchId, orderedIds, displayTerms, request);
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

        if (request.CategoryId.HasValue)
            baseQuery = ApplyCategoryFilter(baseQuery, request.CategoryId.Value);

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

    /// <summary>
    /// 命中分类自身或其直接子分类下的商品（一级分类可覆盖其二级子分类）
    /// </summary>
    private static IQueryable<Product> ApplyCategoryFilter(IQueryable<Product> query, long categoryId)
        => query.Where(p => p.CategoryId == categoryId
            || (p.Category != null && p.Category.ParentId == categoryId));

    private SearchResultDto NewEmptyResult(SearchRequestDto request) => new()
    {
        SearchId = string.IsNullOrEmpty(request.SortBy) || request.SortBy == "relevance"
            ? _cache.Store(request, [], []) : "",
        Items = new(),
        TotalCount = 0,
        Page = request.Page,
        PageSize = request.PageSize
    };

    private async Task<SearchResultDto> BuildCachedResultAsync(SearchResultCache.Snapshot snapshot,
        SearchRequestDto request)
        => await BuildResultFromIdsAsync(snapshot.SearchId, snapshot.ProductIds,
            snapshot.ExpandedTerms, request);

    private async Task<SearchResultDto> BuildResultFromIdsAsync(
        string searchId, IReadOnlyList<long> allIds, IReadOnlyList<string> expandedTerms,
        SearchRequestDto request)
    {
        var pageIds = allIds.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize).ToList();
        var items = await LoadCardsAsync(pageIds);
        return new SearchResultDto
        {
            SearchId = searchId,
            Items = items,
            TotalCount = allIds.Count,
            Page = request.Page,
            PageSize = request.PageSize,
            ExpandedTerms = expandedTerms.ToList()
        };
    }

    private async Task<List<ProductCardDto>> LoadCardsAsync(IReadOnlyList<long> ids)
    {
        if (ids.Count == 0) return new List<ProductCardDto>();
        var products = await _db.Products.AsNoTracking()
            .Where(p => ids.Contains(p.ProductId))
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();
        var viewCounts = await _productViewRepo.GetViewCountsAsync(products.Select(p => p.ProductId));
        var order = ids.Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);
        return products
            .OrderBy(p => order[p.ProductId])
            .Select(p => ProductService.ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();
    }

    private static List<WeightedTerm> MergeSimilarTerms(
        IReadOnlyList<string> originalTerms, SimilaritySnapshot snapshot)
    {
        var originalSet = originalTerms.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var merged = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var original in originalTerms)
        {
            if (!snapshot.Terms.TryGetValue(original, out var terms)) continue;
            foreach (var term in terms)
            {
                if (originalSet.Contains(term.Term)) continue;
                if (!double.IsFinite(term.Similarity) || term.Similarity <= 0) continue;
                merged[term.Term] = Math.Max(merged.GetValueOrDefault(term.Term), term.Similarity);
            }
        }
        return merged
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => new WeightedTerm(x.Key, x.Value))
            .ToList();
    }

    private static bool Contains(string? name, string? info, string term) =>
        (name?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
        (info?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false);

    private static double ComputeSimilarTermScore(string? name, string? info,
        IReadOnlyList<WeightedTerm> similarTerms)
    {
        var score = 0.0;
        foreach (var term in similarTerms)
        {
            if (name?.Contains(term.Term, StringComparison.OrdinalIgnoreCase) == true)
                score += 2.0 * term.Similarity;
            if (info?.Contains(term.Term, StringComparison.OrdinalIgnoreCase) == true)
                score += term.Similarity;
        }
        return score;
    }

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

}
