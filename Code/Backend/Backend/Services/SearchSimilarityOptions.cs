namespace Backend.Services;

/// <summary>搜索相似度维护配置。</summary>
public sealed class SearchSimilarityOptions
{
    public bool Enabled { get; set; } = true;
    public string MaintenanceTime { get; set; } = "03:00";
    public string TimeZoneId { get; set; } = "Asia/Hong_Kong";
    public int TopK { get; set; } = 3;
    public double MinTermTotalWeight { get; set; } = 20.0;
    public int RetryDelayMinutes { get; set; } = 30;
}

