using Backend.Dtos.Product;

namespace Backend.Services;

public sealed class SearchResultCache
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);
    private const int MaxCacheCount = 200;
    private readonly object _lock = new();
    private readonly Dictionary<string, Entry> _entries = new();
    private readonly TimeProvider _clock;

    public SearchResultCache(TimeProvider clock) => _clock = clock;

    private sealed record Entry(string Keyword, int? UserId, long? CategoryId, long[] ProductIds,
        List<string> ExpandedTerms)
    {
        public DateTimeOffset LastAccess { get; set; }
    }

    public sealed record Snapshot(string SearchId, long[] ProductIds, List<string> ExpandedTerms);

    public string Store(SearchRequestDto request, IReadOnlyList<long> productIds,
        IReadOnlyList<string> expandedTerms)
    {
        lock (_lock)
        {
            PurgeExpired();
            while (_entries.Count >= MaxCacheCount)
                _entries.Remove(_entries.MinBy(pair => pair.Value.LastAccess).Key);

            var id = Guid.NewGuid().ToString("N");
            _entries[id] = new Entry(request.Keyword.Trim(), request.UserId, request.CategoryId,
                productIds.ToArray(), expandedTerms.ToList())
            {
                LastAccess = _clock.GetUtcNow()
            };
            return id;
        }
    }

    public Snapshot? TryGet(SearchRequestDto request)
    {
        lock (_lock)
        {
            PurgeExpired();
            if (string.IsNullOrWhiteSpace(request.SearchId) ||
                !_entries.TryGetValue(request.SearchId, out var entry) ||
                entry.UserId != request.UserId ||
                entry.CategoryId != request.CategoryId ||
                (!string.IsNullOrWhiteSpace(request.Keyword) &&
                 !string.Equals(entry.Keyword, request.Keyword.Trim(), StringComparison.Ordinal)) ||
                (!string.IsNullOrEmpty(request.SortBy) && request.SortBy != "relevance"))
                return null;

            entry.LastAccess = _clock.GetUtcNow();
            return new Snapshot(request.SearchId, entry.ProductIds.ToArray(), entry.ExpandedTerms.ToList());
        }
    }

    private void PurgeExpired()
    {
        var cutoff = _clock.GetUtcNow() - CacheTtl;
        foreach (var id in _entries.Where(pair => pair.Value.LastAccess <= cutoff)
                     .Select(pair => pair.Key).ToList())
            _entries.Remove(id);
    }
}
