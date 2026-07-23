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

    // ==================== 搜索入口 ====================

    public async Task<SearchResultDto> SearchAsync(SearchRequestDto request)
    {
        // 1. 分词
        var rawTerms = _termExtraction.Extract(request.Keyword);

        if (rawTerms.Count == 0)
        {
            _logger.LogDebug("Search keyword produced no terms, returning empty result");
            return new SearchResultDto { Items = new(), TotalCount = 0, Page = request.Page, PageSize = request.PageSize };
        }

        // 2. 查询扩展（若图已初始化）
        List<(string term, double weight)> expandedTerms;
        if (_termGraph.IsInitialized)
        {
            expandedTerms = _termGraph.ExpandQuery(rawTerms);
        }
        else
        {
            // 冷启动降级：仅用原始词条
            expandedTerms = rawTerms.Select(t => (t, 1.0)).ToList();
            _logger.LogWarning("TermGraph not initialized, using raw terms only for search");
        }

        var termList = expandedTerms.Select(t => t.term).ToList();

        // 3. 数据库搜索
        var baseQuery = _db.Products
            .Where(p => p.Status == ProductStatus.Available);

        // 动态构建 OR 表达式
        var filterExpression = BuildOrFilter(termList);
        if (filterExpression != null)
            baseQuery = baseQuery.Where(filterExpression);

        // 4. 排序
        var orderedQuery = ApplySorting(baseQuery, request.SortBy);

        // 5. 计数 & 分页
        var totalCount = await orderedQuery.CountAsync();
        var products = await orderedQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

        // 6. 批量查浏览量
        var productIds = products.Select(p => p.ProductId).ToList();
        var viewCounts = await _productViewRepo.GetViewCountsAsync(productIds);

        // 7. 映射 DTO
        var items = products.Select(p =>
        {
            var viewCount = viewCounts.GetValueOrDefault(p.ProductId, 0);
            var relevance = ComputeRelevanceScore(p, expandedTerms);
            return ToSearchCard(p, viewCount, relevance);
        }).ToList();

        // 若按相关性排序，在内存中重排
        if (string.IsNullOrEmpty(request.SortBy) || request.SortBy == "relevance")
            items = items.OrderByDescending(i => i.RelevanceScore).ToList();

        return new SearchResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            ExpandedTerms = termList
        };
    }

    // ==================== 新商品通知 ====================

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

        // 增量持久化到数据库
        await _termGraph.SaveToDatabaseAsync();

        _logger.LogDebug("Processed new product {ProductId} with {TermCount} terms", productId, terms.Count);
    }

    // ==================== 全量重建 ====================

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

        // 全量重建后持久化（SaveToDatabaseAsync 内部通过脏标记自动做增量 upsert）
        await _termGraph.SaveToDatabaseAsync();
    }

    // ==================== 辅助方法 ====================

    /// <summary>
    /// 动态构建 OR 过滤表达式：
    /// p => (p.Name.Contains(t1) || (p.Info != null && p.Info.Contains(t1)))
    ///    || (p.Name.Contains(t2) || (p.Info != null && p.Info.Contains(t2)))
    ///    || ...
    /// </summary>
    private static Expression<Func<Product, bool>>? BuildOrFilter(List<string> terms)
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

            // p.Name.Contains(term)
            var nameContains = Expression.Call(nameProp, containsMethod, termConst);

            // p.Info != null && p.Info.Contains(term)
            var infoNotNull = Expression.NotEqual(infoProp, nullConst);
            var infoContains = Expression.Call(infoProp, containsMethod, termConst);
            var infoMatch = Expression.AndAlso(infoNotNull, infoContains);

            // Name match OR Info match
            var termMatch = Expression.OrElse(nameContains, infoMatch);

            // 用 OR 连接所有词条
            body = body == null ? termMatch : Expression.OrElse(body, termMatch);
        }

        return body != null
            ? Expression.Lambda<Func<Product, bool>>(body, parameter)
            : null;
    }

    /// <summary>
    /// 排序策略
    /// </summary>
    private static IOrderedQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy)
    {
        return sortBy switch
        {
            "latest" => query.OrderByDescending(p => p.ReleaseDate),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderByDescending(p => p.ReleaseDate) // relevance 排序在内存中做
        };
    }

    /// <summary>
    /// 计算相关性分数（Phase 1 简单版：匹配词条加权求和）
    /// Name 命中权重 2.0，Info 命中权重 1.0
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

        return (int)(score * 100); // 放大为整数便于排序
    }

    private static SearchProductCardDto ToSearchCard(Product p, int viewCount, int relevanceScore) => new()
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
        ViewCount = viewCount,
        RelevanceScore = relevanceScore
    };
}
