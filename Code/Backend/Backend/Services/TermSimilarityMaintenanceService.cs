using Microsoft.Extensions.Options;

namespace Backend.Services;

public sealed class TermSimilarityMaintenanceService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<SearchSimilarityOptions> _options;
    private readonly ILogger<TermSimilarityMaintenanceService> _logger;

    public TermSimilarityMaintenanceService(
        IServiceScopeFactory scopeFactory,
        IOptions<SearchSimilarityOptions> options,
        ILogger<TermSimilarityMaintenanceService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Value.Enabled) return;

        // 让 Web 应用先完成监听；空库首次计算在后台执行。
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        using (var initialScope = _scopeFactory.CreateScope())
        {
            var store = initialScope.ServiceProvider.GetRequiredService<ITermSimilarityStore>();
            if (store.Current.Terms.Count == 0)
            {
                try
                {
                    var refresh = initialScope.ServiceProvider.GetRequiredService<ITermSimilarityRefreshService>();
                    await refresh.RefreshAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Initial search similarity refresh failed");
                }
            }
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = DelayUntilNextMaintenance();
                await Task.Delay(delay, stoppingToken);
                using var scope = _scopeFactory.CreateScope();
                var refresh = scope.ServiceProvider.GetRequiredService<ITermSimilarityRefreshService>();
                await refresh.RefreshAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled search similarity refresh failed");
                try { await Task.Delay(TimeSpan.FromMinutes(_options.Value.RetryDelayMinutes), stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }
    }

    private TimeSpan DelayUntilNextMaintenance()
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById(_options.Value.TimeZoneId); }
        catch { zone = TimeZoneInfo.Utc; }

        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone);
        if (!TimeSpan.TryParse(_options.Value.MaintenanceTime, out var time))
            time = new TimeSpan(3, 0, 0);
        var next = now.Date.Add(time);
        if (next <= now.DateTime) next = next.AddDays(1);
        var nextOffset = new DateTimeOffset(next, now.Offset);
        return nextOffset - now;
    }
}
