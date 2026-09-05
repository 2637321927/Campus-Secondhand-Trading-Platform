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

    private sealed record Entry(string Keyword, int? UserId, ProductCardDto[] Items,
        List<string> ExpandedTerms)
    {
        public DateTimeOffset LastAccess { get; set; }
    }

    public string Store(SearchRequestDto request, ProductCardDto[] items, List<string> expandedTerms)
    {
        lock (_lock)
        {
            PurgeExpired();
            while (_entries.Count >= MaxCacheCount)
                _entries.Remove(_entries.MinBy(pair => pair.Value.LastAccess).Key);

            var id = Guid.NewGuid().ToString("N");
            _entries[id] = new Entry(request.Keyword.Trim(), request.UserId, items, expandedTerms)
            {
                LastAccess = _clock.GetUtcNow()
            };
            return id;
        }
    }

    public SearchResultDto? TryGet(SearchRequestDto request)
    {
        lock (_lock)
        {
            PurgeExpired();
            if (string.IsNullOrWhiteSpace(request.SearchId) ||
                !_entries.TryGetValue(request.SearchId, out var entry) ||
                entry.UserId != request.UserId ||
                (!string.IsNullOrWhiteSpace(request.Keyword) &&
                 !string.Equals(entry.Keyword, request.Keyword.Trim(), StringComparison.Ordinal)) ||
                (!string.IsNullOrEmpty(request.SortBy) && request.SortBy != "relevance"))
                return null;

            entry.LastAccess = _clock.GetUtcNow();
            return new SearchResultDto
            {
                SearchId = request.SearchId,
                Items = entry.Items.Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize).ToList(),
                TotalCount = entry.Items.Length,
                Page = request.Page,
                PageSize = request.PageSize,
                ExpandedTerms = entry.ExpandedTerms.ToList()
            };
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
