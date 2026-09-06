using Backend.Utilities;
using Microsoft.Extensions.Options;

namespace Backend.Services;

/// <summary>根据当前词图快照生成近似词库。</summary>
public sealed class TermSimilarityRefreshService : ITermSimilarityRefreshService
{
    private readonly TermGraph _termGraph;
    private readonly ITermSimilarityCalculator _calculator;
    private readonly ITermSimilarityStore _store;
    private readonly ILogger<TermSimilarityRefreshService> _logger;
    private readonly IOptions<SearchSimilarityOptions> _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public TermSimilarityRefreshService(
        TermGraph termGraph,
        ITermSimilarityCalculator calculator,
        ITermSimilarityStore store,
        ILogger<TermSimilarityRefreshService> logger,
        IOptions<SearchSimilarityOptions> options)
    {
        _termGraph = termGraph;
        _calculator = calculator;
        _store = store;
        _logger = logger;
        _options = options;
    }

    public bool IsRunning => _gate.CurrentCount == 0;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var started = DateTime.UtcNow;
            var snapshot = _termGraph.CreateSnapshot();
            var values = _calculator.Calculate(snapshot,
                _options.Value.MinTermTotalWeight,
                _options.Value.TopK,
                cancellationToken);
            await _store.ReplaceAsync(values, cancellationToken);
            _logger.LogInformation(
                "Search term similarity refreshed: {Terms} graph nodes, {Sources} source terms, {Relations} relations, {ElapsedMs} ms",
                snapshot.Adjacency.Count, values.Count, values.Sum(x => x.Value.Count),
                (DateTime.UtcNow - started).TotalMilliseconds);
        }
        finally
        {
            _gate.Release();
        }
    }
}
