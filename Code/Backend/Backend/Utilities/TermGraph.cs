using System.Collections.Concurrent;
using Backend.Data;
using Backend.Models;
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
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, double>> _adjacency = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>行权重和</summary>
    private readonly ConcurrentDictionary<string, double> _rowSums = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>卖家索引</summary>
    private readonly ConcurrentDictionary<int, HashSet<string>> _sellerTerms = new();

    /// <summary>分类索引</summary>
    private readonly ConcurrentDictionary<long, HashSet<string>> _categoryTerms = new();

    private readonly IServiceScopeFactory _scopeFactory;
    private volatile bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    /// <summary>脏词条集合</summary>
    private readonly ConcurrentDictionary<string, bool> _dirtyTerms = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>脏边集合</summary>
    private readonly ConcurrentDictionary<EdgeKey, byte> _dirtyEdges = new();

    private volatile bool _hasDirtyData;

    private readonly SemaphoreSlim _saveLock = new(1, 1);
    public double SameProductWeight { get; set; } = 3.0;
    public double SameSellerWeight { get; set; } = 1.0;
    public double SameCategoryWeight { get; set; } = 0.5;
    public int MaxExpandedPerTerm { get; set; } = 3;
    public double MinRelatedWeight { get; set; } = 0.05;
    public int MaxTotalTerms { get; set; } = 15;

    public bool IsInitialized => _isInitialized;
    public int NodeCount => _adjacency.Count;
    public int EdgeCount => _adjacency.Values.Sum(v => v.Count) / 2;

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
        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized) return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var edges = await db.SearchTermEdges
                .Include(e => e.Term1)
                .Include(e => e.Term2)
                .AsNoTracking()
                .ToListAsync();

            foreach (var edge in edges)
            {
                if (edge.Term1 == null || edge.Term2 == null) continue;
                AddToMemory(edge.Term1.TermText, edge.Term2.TermText, edge.Weight);
            }

            _dirtyTerms.Clear();
            _dirtyEdges.Clear();
            _hasDirtyData = false;

            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// 添加一条共现边
    /// </summary>
    public void AddCoOccurrence(string term1, string term2, double weight)
    {
        if (string.IsNullOrEmpty(term1) || string.IsNullOrEmpty(term2)) return;
        if (term1.Equals(term2, StringComparison.OrdinalIgnoreCase)) return;

        AddToMemory(term1, term2, weight);
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

        // 标记脏数据
        _dirtyTerms[term1] = true;
        _dirtyTerms[term2] = true;
        _dirtyEdges[EdgeKey.Create(term1, term2)] = 0;
        _hasDirtyData = true;
    }

    /// <summary>
    /// 查询某个词条的Top-K相关词条
    /// </summary>
    public List<(string term, double normalizedWeight)> GetRelatedTerms(string term, int k, double minWeight = 0.05)
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
        return _adjacency.ContainsKey(term);
    }

    /// <summary>
    /// 为每个词条获取Top-K相关词条，合并去重返回
    /// </summary>
    public List<(string term, double weight)> ExpandQuery(List<string> terms, int? maxPerTerm = null, int? maxTotal = null)
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
    public void ProcessNewProduct(List<string> terms, int sellerId, long categoryId)
    {
        if (terms.Count < 1) return;

        for (var i = 0; i < terms.Count; i++)
            for (var j = i + 1; j < terms.Count; j++)
                AddCoOccurrence(terms[i], terms[j], SameProductWeight);

        var sellerSet = _sellerTerms.GetOrAdd(sellerId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var existingSellerTerms = sellerSet.ToList();
        foreach (var t in terms)
        {
            foreach (var existingTerm in existingSellerTerms)
            {
                if (!t.Equals(existingTerm, StringComparison.OrdinalIgnoreCase))
                    AddCoOccurrence(t, existingTerm, SameSellerWeight);
            }
        }
        foreach (var t in terms)
            sellerSet.Add(t);

        var catSet = _categoryTerms.GetOrAdd(categoryId, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        var existingCatTerms = catSet.ToList();
        foreach (var t in terms)
        {
            foreach (var existingTerm in existingCatTerms)
            {
                if (!t.Equals(existingTerm, StringComparison.OrdinalIgnoreCase))
                    AddCoOccurrence(t, existingTerm, SameCategoryWeight);
            }
        }
        foreach (var t in terms)
            catSet.Add(t);
    }

    /// <summary>
    /// 将内存中的脏数据增量持久化到数据库
    /// </summary>
    public async Task SaveToDatabaseAsync()
    {
        if (!_hasDirtyData) return;

        await _saveLock.WaitAsync();
        try
        {
            if (!_hasDirtyData) return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dirtyTermList = _dirtyTerms.Keys.ToList();
            var dirtyEdgeKeys = _dirtyEdges.Keys.ToList();

            var existingTerms = await db.SearchTerms
                .Where(t => dirtyTermList.Contains(t.TermText))
                .AsTracking()
                .ToListAsync();

            var existingTermDict = existingTerms.ToDictionary(
                t => t.TermText, t => t, StringComparer.OrdinalIgnoreCase);

            var newTerms = new List<SearchTerm>();
            foreach (var termText in dirtyTermList)
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

            var allDirtyTerms = await db.SearchTerms
                .Where(t => dirtyTermList.Contains(t.TermText))
                .AsNoTracking()
                .ToListAsync();

            var termToId = allDirtyTerms.ToDictionary(
                t => t.TermText, t => t.TermId, StringComparer.OrdinalIgnoreCase);

            var dirtyPairs = dirtyEdgeKeys
                .Where(p => termToId.ContainsKey(p.Term1) && termToId.ContainsKey(p.Term2))
                .Select(p =>
                {
                    var id1 = Math.Min(termToId[p.Term1], termToId[p.Term2]);
                    var id2 = Math.Max(termToId[p.Term1], termToId[p.Term2]);
                    return (id1, id2, t1: p.Term1, t2: p.Term2);
                })
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

            _dirtyTerms.Clear();
            _dirtyEdges.Clear();
            _hasDirtyData = false;
        }
        finally
        {
            _saveLock.Release();
        }
    }
}

internal readonly record struct EdgeKey(string Term1, string Term2)
{
    public static EdgeKey Create(string t1, string t2)
    {
        return string.Compare(t1, t2, StringComparison.OrdinalIgnoreCase) < 0
            ? new EdgeKey(t1, t2)
            : new EdgeKey(t2, t1);
    }
}
