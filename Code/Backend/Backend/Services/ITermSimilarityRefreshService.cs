namespace Backend.Services;

public interface ITermSimilarityRefreshService
{
    bool IsRunning { get; }
    Task RefreshAsync(CancellationToken cancellationToken = default);
}

