using Backend.Data;
using Backend.Models;
using Backend.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>近似词库的持久化和进程内只读快照。</summary>
public sealed class TermSimilarityStore : ITermSimilarityStore
{
    private readonly IServiceScopeFactory _scopeFactory;
    private SimilaritySnapshot _current = SimilaritySnapshot.Empty;

    public TermSimilarityStore(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public SimilaritySnapshot Current => Volatile.Read(ref _current);

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var rows = await db.SearchTermSimilarities
                .AsNoTracking()
                .Include(x => x.SourceTerm)
                .Include(x => x.SimilarTerm)
                .OrderBy(x => x.SourceTermId)
                .ThenBy(x => x.Rank)
                .ToListAsync(cancellationToken);
            SetCurrent(ToSnapshot(rows));
        }
        catch (Exception ex)
        {
            // 迁移尚未部署时不阻塞应用启动，搜索退化为原词排序。
            Console.Error.WriteLine($"Failed to load search term similarities: {ex.Message}");
            SetCurrent(SimilaritySnapshot.Empty);
        }
    }

    public async Task ReplaceAsync(IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> values,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var terms = await db.SearchTerms.AsNoTracking().ToListAsync(cancellationToken);
        var termIds = terms.ToDictionary(x => x.TermText, x => x.TermId, StringComparer.OrdinalIgnoreCase);

        db.SearchTermSimilarities.RemoveRange(await db.SearchTermSimilarities.ToListAsync(cancellationToken));
        var rows = new List<SearchTermSimilarity>();
        foreach (var source in values)
        {
            if (!termIds.TryGetValue(source.Key, out var sourceId)) continue;
            var rank = 1;
            foreach (var similar in source.Value.Take(3))
            {
                if (!termIds.TryGetValue(similar.Term, out var similarId) || sourceId == similarId)
                    continue;
                rows.Add(new SearchTermSimilarity
                {
                    SourceTermId = sourceId,
                    SimilarTermId = similarId,
                    Similarity = similar.Similarity,
                    Rank = rank++,
                    UpdatedAt = DateTime.Now
                });
            }
        }

        if (rows.Count > 0)
            await db.SearchTermSimilarities.AddRangeAsync(rows, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        SetCurrent(new SimilaritySnapshot(Clone(values)));
    }

    private static SimilaritySnapshot ToSnapshot(IEnumerable<SearchTermSimilarity> rows)
    {
        var values = new Dictionary<string, IReadOnlyList<WeightedTerm>>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in rows.Where(x => x.SourceTerm != null && x.SimilarTerm != null)
                     .GroupBy(x => x.SourceTerm!.TermText, StringComparer.OrdinalIgnoreCase))
        {
            values[group.Key] = group.OrderBy(x => x.Rank)
                .Select(x => new WeightedTerm(x.SimilarTerm!.TermText, x.Similarity))
                .ToList();
        }
        return new SimilaritySnapshot(values);
    }

    private void SetCurrent(SimilaritySnapshot snapshot) => Interlocked.Exchange(ref _current, snapshot);

    private static IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> Clone(
        IReadOnlyDictionary<string, IReadOnlyList<WeightedTerm>> values)
    {
        return values.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<WeightedTerm>)pair.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);
    }
}
