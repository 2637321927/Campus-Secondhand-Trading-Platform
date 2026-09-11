namespace Backend.Services;

/// <summary>
/// 后台定时服务：自动取消超过指定时长仍未付款的订单
/// </summary>
public sealed class OrderAutoCancelService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderAutoCancelService> _logger;

    public OrderAutoCancelService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<OrderAutoCancelService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 读取配置，带默认值兜底
        var expireMinutes = _configuration.GetValue("OrderAutoCancel:ExpireMinutes", 5);
        var scanIntervalSeconds = _configuration.GetValue("OrderAutoCancel:ScanIntervalSeconds", 30);
        var expiration = TimeSpan.FromMinutes(expireMinutes);
        var scanInterval = TimeSpan.FromSeconds(scanIntervalSeconds);

        _logger.LogInformation(
            "订单自动取消服务已启动：超时 {Expire} 分钟，扫描间隔 {Scan} 秒",
            expireMinutes, scanIntervalSeconds);

        // 启动后先等一轮，让 Web 服务先就绪
        try { await Task.Delay(scanInterval, stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.CancelExpiredOrdersAsync(expiration);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自动取消超时订单执行失败");
            }

            try { await Task.Delay(scanInterval, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }
}
