using System.Collections.Concurrent;
using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Utilities;

/// <summary>
/// ItemRank词条关联图
/// 节点：每个独立词条
/// 边：加权无向边，权重表示两个词条的关联强度
/// </summary>
public class TermGraph
{

    /// <summary>邻接表</summary>
    private ConcurrentDictionary<string, ConcurrentDictionary<string, double>> _adjacency = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>行权重和</summary>
    private ConcurrentDictionary<string, double> _rowSums = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>卖家索引</summary>
    private ConcurrentDictionary<int, HashSet<string>> _sellerTerms = new();

    /// <summary>分类索引</summary>
    private ConcurrentDictionary<long, HashSet<string>> _categoryTerms = new();
    private HashSet<long> _indexedProductIds = new();
    private readonly object _graphLock = new();

    private readonly IServiceScopeFactory _scopeFactory;
    private volatile bool _isInitialized;
    // 串行化初始化、重建以及“商品入图 + 持久化”，避免相互覆盖。
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    public double SameProductWeight { get; set; } = 3.0;
    public double SameSellerWeight { get; set; } = 1.0;
    public double SameCategoryWeight { get; set; } = 0.5;
    public int MaxExpandedPerTerm { get; set; } = 3;
    public double MinRelatedWeight { get; set; } = 0.05;
    public int MaxTotalTerms { get; set; } = 15;

    public bool IsInitialized => _isInitialized;
    public int NodeCount { get { lock (_graphLock) return _adjacency.Count; } }
    public int EdgeCount { get { lock (_graphLock) return _adjacency.Values.Sum(v => v.Count) / 2; } }

    public TermGraph(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>
    /// 从数据库加载已持久化的图结构，若为空则标记为已初始化
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        await _saveLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var snapshot = CreateEmptySnapshot();
            var terms = await db.SearchTerms.AsNoTracking().ToListAsync();
            foreach (var term in terms)
                snapshot.EnsureNode(term.TermText);

            var edges = await db.SearchTermEdges
                .Include(e => e.Term1)
                .Include(e => e.Term2)
                .AsNoTracking()
                .ToListAsync();

            foreach (var edge in edges)
            {
                if (edge.Term1 == null || edge.Term2 == null) continue;
                snapshot.AddToMemory(edge.Term1.TermText, edge.Term2.TermText, edge.Weight);
            }

            // 历史集合只恢复成员，不再次累加已经持久化的边。
            var extractor = scope.ServiceProvider.GetRequiredService<ITermExtractionService>();
            foreach (var product in await LoadProductsAsync(db))
                snapshot.RestoreProductMembership(product, extractor.Extract($"{product.Name} {product.Info ?? ""}"));

            Publish(snapshot);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    public async Task RebuildAsync(ITermExtractionService extractor)
    {
        await _saveLock.WaitAsync();
        try
        {
            await RebuildCoreAsync(extractor);
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private async Task RebuildCoreAsync(ITermExtractionService extractor)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var snapshot = BuildSnapshot(await LoadProductsAsync(db), extractor);
        var edges = snapshot._adjacency.SelectMany(pair => pair.Value.Keys
            .Select(neighbor => EdgeKey.Create(pair.Key, neighbor))).Distinct().ToList();

        // 事务成功之前保持当前图可用；空商品库也必须清除旧图。
        await snapshot.SaveIncrementalCoreAsync(snapshot._adjacency.Keys.ToList(), edges, replace: true);
        Publish(snapshot);
    }

    public async Task ProcessAndSaveProductAsync(long productId, ITermExtractionService extractor)
    {
        await _saveLock.WaitAsync();
        try
        {
            if (!_isInitialized)
            {
                await RebuildCoreAsync(extractor);
                return;
            }

            // 重建可能已包含刚提交的商品，通知不能将它重复入图。
            if (_indexedProductIds.Contains(productId)) return;
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var product = await db.Products.AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null || product.Status == ProductStatus.Removed) return;

            var terms = extractor.Extract($"{product.Name} {product.Info ?? ""}");
            var (affectedTerms, affectedEdges) = ProcessNewProduct(terms, product.UserId, product.CategoryId);
            try
            {
                await SaveIncrementalCoreAsync(affectedTerms, affectedEdges);
                _indexedProductIds.Add(productId);
            }
            catch
            {
                // 持久化失败后不继续使用未提交的图；下次更新或手动重建会恢复。
                _isInitialized = false;
                throw;
            }
        }
        finally
        {
            _saveLock.Release();
        }
    }

    private static Task<List<Product>> LoadProductsAsync(AppDbContext db) => db.Products
        .AsNoTracking()
        .Where(p => p.Status != ProductStatus.Removed)
        .OrderBy(p => p.ProductId)
        .Select(p => new Product
        {
            ProductId = p.ProductId, Name = p.Name, Info = p.Info,
            UserId = p.UserId, CategoryId = p.CategoryId
        }).ToListAsync();

    private TermGraph CreateEmptySnapshot() => new(_scopeFactory)
    {
        SameProductWeight = SameProductWeight,
        SameSellerWeight = SameSellerWeight,
        SameCategoryWeight = SameCategoryWeight,
        MaxExpandedPerTerm = MaxExpandedPerTerm,
        MinRelatedWeight = MinRelatedWeight,
        MaxTotalTerms = MaxTotalTerms
    };

    internal TermGraph BuildSnapshot(IEnumerable<Product> products, ITermExtractionService extractor)
    {
        var snapshot = CreateEmptySnapshot();
        foreach (var product in products.OrderBy(p => p.ProductId))
        {
            var terms = extractor.Extract($"{product.Name} {product.Info ?? ""}");
            snapshot.ProcessNewProduct(terms, product.UserId, product.CategoryId);
            snapshot._indexedProductIds.Add(product.ProductId);
        }
        return snapshot;
    }

    internal void RestoreProductMembership(Product product, List<string> terms)
    {
        lock (_graphLock)
        {
            _sellerTerms.GetOrAdd(product.UserId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase))
                .UnionWith(terms);
            _categoryTerms.GetOrAdd(product.CategoryId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase))
                .UnionWith(terms);
            _indexedProductIds.Add(product.ProductId);
        }
    }

    internal void Publish(TermGraph snapshot)
    {
        lock (_graphLock)
        {
            _adjacency = snapshot._adjacency;
            _rowSums = snapshot._rowSums;
            _sellerTerms = snapshot._sellerTerms;
            _categoryTerms = snapshot._categoryTerms;
            _indexedProductIds = snapshot._indexedProductIds;
            _isInitialized = true;
        }
    }

    private void EnsureNode(string term)
    {
        _adjacency.GetOrAdd(term, _ => new ConcurrentDictionary<string, double>(StringComparer.OrdinalIgnoreCase));
        _rowSums.TryAdd(term, 0);
    }

    /// <summary>
    /// 添加一条共现边
    /// </summary>
    public EdgeKey AddCoOccurrence(string term1, string term2, double weight)
    {
        lock (_graphLock) return AddCoOccurrenceCore(term1, term2, weight);
    }

    private EdgeKey AddCoOccurrenceCore(string term1, string term2, double weight)
    {
        if (string.IsNullOrEmpty(term1) || string.IsNullOrEmpty(term2))
            return default;
        if (term1.Equals(term2, StringComparison.OrdinalIgnoreCase))
            return default;

        AddToMemory(term1, term2, weight);
        return EdgeKey.Create(term1, term2);
    }

    private void AddToMemory(string term1, string term2, double weight)
    {

        // 新增/修改边权重
        var neighbors1 = _adjacency.GetOrAdd(term1, _ => new ConcurrentDictionary<string, double>(StringComparer.OrdinalIgnoreCase));
        neighbors1.AddOrUpdate(term2, weight, (_, old) => old + weight);

        var neighbors2 = _adjacency.GetOrAdd(term2, _ => new ConcurrentDictionary<string, double>(StringComparer.OrdinalIgnoreCase));
        neighbors2.AddOrUpdate(term1, weight, (_, old) => old + weight);

        _rowSums.AddOrUpdate(term1, weight, (_, old) => old + weight);
        _rowSums.AddOrUpdate(term2, weight, (_, old) => old + weight);
    }

    /// <summary>
    /// 查询某个词条的Top-K相关词条
    /// </summary>
    public List<(string term, double normalizedWeight)> GetRelatedTerms(string term, int k, double minWeight = 0.05)
    {
        lock (_graphLock) return GetRelatedTermsCore(term, k, minWeight);
    }

    private List<(string term, double normalizedWeight)> GetRelatedTermsCore(string term, int k, double minWeight)
    {
        if (!_adjacency.TryGetValue(term, out var neighbors))
            return new List<(string, double)>();

        var rowSum = _rowSums.GetValueOrDefault(term, 1.0);
        if (rowSum <= 0) rowSum = 1.0;

        return neighbors
            .Select(kv => (term: kv.Key, normalizedWeight: kv.Value / rowSum))
            .Where(x => x.normalizedWeight >= minWeight)
            .OrderByDescending(x => x.normalizedWeight)
            .Take(k)
            .ToList();
    }

    /// <summary>
    /// 检查图中是否已存在该词条节点
    /// </summary>
    public bool ContainsTerm(string term)
    {
        lock (_graphLock) return _adjacency.ContainsKey(term);
    }

    /// <summary>
    /// 为每个词条获取Top-K相关词条，合并去重返回
    /// </summary>
    public List<(string term, double weight)> ExpandQuery(List<string> terms, int? maxPerTerm = null, int? maxTotal = null)
    {
        lock (_graphLock) return ExpandQueryCore(terms, maxPerTerm, maxTotal);
    }

    private List<(string term, double weight)> ExpandQueryCore(List<string> terms, int? maxPerTerm, int? maxTotal)
    {
        var mp = maxPerTerm ?? MaxExpandedPerTerm;
        var mt = maxTotal ?? MaxTotalTerms;

        var result = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in terms)
            result[t] = 1.0;

        foreach (var t in terms)
        {
            var related = GetRelatedTerms(t, mp, MinRelatedWeight);
            foreach (var (rTerm, rWeight) in related)
            {
                if (result.TryGetValue(rTerm, out var existing))
                    result[rTerm] = Math.Max(existing, rWeight);
                else
                    result[rTerm] = rWeight;
            }
        }

        return result
            .OrderByDescending(kv => kv.Value)
            .Take(mt)
            .Select(kv => (kv.Key, kv.Value))
            .ToList();
    }

    /// <summary>
    /// 处理新商品：对词条集合添加三层边
    /// </summary>
    public (List<string> AffectedTerms, List<EdgeKey> AffectedEdges) ProcessNewProduct(
        List<string> terms, int sellerId, long categoryId)
    {
        lock (_graphLock) return ProcessNewProductCore(terms, sellerId, categoryId);
    }

    private (List<string> AffectedTerms, List<EdgeKey> AffectedEdges) ProcessNewProductCore(
        List<string> terms, int sellerId, long categoryId)
    {
        terms = terms.Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var affectedEdges = new List<EdgeKey>();
        var affectedTerms = new HashSet<string>(terms, StringComparer.OrdinalIgnoreCase);

        if (terms.Count < 1)
            return (new List<string>(), new List<EdgeKey>());

        foreach (var term in terms) EnsureNode(term);

        // 同商品词条间共现
        for (var i = 0; i < terms.Count; i++)
        {
            for (var j = i + 1; j < terms.Count; j++)
            {
                var edge = AddCoOccurrence(terms[i], terms[j], SameProductWeight);
                if (!edge.Equals(default(EdgeKey)))
                    affectedEdges.Add(edge);
            }
        }

        // 同卖家词条关联
        var sellerSet = _sellerTerms.GetOrAdd(sellerId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var existingSellerTerms = sellerSet.ToList();
        foreach (var t in terms)
        {
            foreach (var existingTerm in existingSellerTerms)
            {
                if (!t.Equals(existingTerm, StringComparison.OrdinalIgnoreCase))
                {
                    var edge = AddCoOccurrence(t, existingTerm, SameSellerWeight);
                    affectedTerms.Add(existingTerm);
                    if (!edge.Equals(default(EdgeKey)))
                        affectedEdges.Add(edge);
                }
            }
        }
        foreach (var t in terms)
            sellerSet.Add(t);

        // 同分类词条关联
        var catSet = _categoryTerms.GetOrAdd(categoryId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var existingCatTerms = catSet.ToList();
        foreach (var t in terms)
        {
            foreach (var existingTerm in existingCatTerms)
            {
                if (!t.Equals(existingTerm, StringComparison.OrdinalIgnoreCase))
                {
                    var edge = AddCoOccurrence(t, existingTerm, SameCategoryWeight);
                    affectedTerms.Add(existingTerm);
                    if (!edge.Equals(default(EdgeKey)))
                        affectedEdges.Add(edge);
                }
            }
        }
        foreach (var t in terms)
            catSet.Add(t);

        return (affectedTerms.ToList(), affectedEdges);
    }

    /// <summary>
    /// 将指定的词条和边增量持久化到数据库
    /// </summary>
    private async Task SaveIncrementalCoreAsync(List<string> terms, List<EdgeKey> edges, bool replace = false)
    {
        if (!replace && terms.Count == 0 && edges.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (replace)
        {
            db.SearchTermEdges.RemoveRange(await db.SearchTermEdges.ToListAsync());
            await db.SaveChangesAsync();
            db.SearchTerms.RemoveRange(await db.SearchTerms.ToListAsync());
            await db.SaveChangesAsync();
        }

        if (terms.Count == 0)
        {
            await transaction.CommitAsync();
            return;
        }

        // 持久化词条节点
        var existingTerms = await db.SearchTerms
            .Where(t => terms.Contains(t.TermText))
            .AsTracking()
            .ToListAsync();

        var existingTermDict = existingTerms.ToDictionary(
            t => t.TermText, t => t, StringComparer.OrdinalIgnoreCase);

        var newTerms = new List<SearchTerm>();
        foreach (var termText in terms)
        {
            if (existingTermDict.TryGetValue(termText, out var entity))
            {
                entity.RowSum = _rowSums.GetValueOrDefault(termText);
                entity.UpdatedAt = DateTime.Now;
            }
            else
            {
                newTerms.Add(new SearchTerm
                {
                    TermText = termText,
                    RowSum = _rowSums.GetValueOrDefault(termText),
                    UpdatedAt = DateTime.Now
                });
            }
        }

        if (newTerms.Count > 0)
            await db.SearchTerms.AddRangeAsync(newTerms);
        await db.SaveChangesAsync();

        // 持久化边
        var allDirtyTerms = await db.SearchTerms
            .Where(t => terms.Contains(t.TermText))
            .AsNoTracking()
            .ToListAsync();

        var termToId = allDirtyTerms.ToDictionary(
            t => t.TermText, t => t.TermId, StringComparer.OrdinalIgnoreCase);

        var dirtyPairs = edges
            .Where(p => termToId.ContainsKey(p.Term1) && termToId.ContainsKey(p.Term2))
            .Select(p =>
            {
                var id1 = Math.Min(termToId[p.Term1], termToId[p.Term2]);
                var id2 = Math.Max(termToId[p.Term1], termToId[p.Term2]);
                return (id1, id2, t1: p.Term1, t2: p.Term2);
            })
            // 同一条边可能重复出现（分词产生重复词条、多个商品共用同一词对），
            // 必须按 (id1, id2) 去重，否则批量插入会违反唯一索引 IX_search_term_edge_term1_id_term2_id
            .GroupBy(p => (p.id1, p.id2))
            .Select(g => g.First())
            .ToList();

        var allDirtyTermIds = termToId.Values.ToList();
        var existingEdges = await db.SearchTermEdges
            .Where(e => allDirtyTermIds.Contains(e.Term1Id)
                     && allDirtyTermIds.Contains(e.Term2Id))
            .AsTracking()
            .ToListAsync();

        var existingEdgeDict = existingEdges.ToDictionary(
            e => (e.Term1Id, e.Term2Id));

        var newEdges = new List<SearchTermEdge>();
        foreach (var (id1, id2, t1, t2) in dirtyPairs)
        {
            var currentWeight = _adjacency.TryGetValue(t1, out var nbrs)
                ? nbrs.GetValueOrDefault(t2, 0.0)
                : 0.0;

            if (currentWeight <= 0) continue;

            if (existingEdgeDict.TryGetValue((id1, id2), out var edge))
            {
                edge.Weight = currentWeight;
                edge.UpdatedAt = DateTime.Now;
            }
            else
            {
                newEdges.Add(new SearchTermEdge
                {
                    Term1Id = id1,
                    Term2Id = id2,
                    Weight = currentWeight,
                    UpdatedAt = DateTime.Now
                });
            }
        }

        if (newEdges.Count > 0)
            await db.SearchTermEdges.AddRangeAsync(newEdges);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

public readonly record struct EdgeKey(string Term1, string Term2)
{
    public static EdgeKey Create(string t1, string t2)
    {
        return string.Compare(t1, t2, StringComparison.OrdinalIgnoreCase) < 0
            ? new EdgeKey(t1, t2)
            : new EdgeKey(t2, t1);
    }
}
