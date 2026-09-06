using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Admin;

/// <summary>
/// 管理员举报/申诉列表项
/// </summary>
public class AdminModerationWorkOrderDto
{
    public long WorkOrderId { get; set; }
    public int Type { get; set; }
    public string? TargetType { get; set; }
    public long? TargetId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Info { get; set; }
    public string Status { get; set; } = "waiting";
    public string? Result { get; set; }
    public string? HandleAction { get; set; }
    public DateTime CreateTime { get; set; }
    public string? Response { get; set; }
    public DateTime? ResponseTime { get; set; }
    public int InitiatorId { get; set; }
    public string InitiatorName { get; set; } = string.Empty;
    public int? AccusedId { get; set; }
    public string? AccusedName { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public long? AppealAgainstWorkOrderId { get; set; }
    public string? AppealAgainstReason { get; set; }
    public int? AdminId { get; set; }
}

/// <summary>
/// 管理员举报/申诉详情
/// </summary>
public class AdminModerationDetailDto : AdminModerationWorkOrderDto
{
    public List<AdminWorkOrderTimelineDto> Timeline { get; set; } = new();
}

public class AdminWorkOrderTimelineDto
{
    public long TimelineId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? AdminId { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 管理员举报/申诉分页结果
/// </summary>
public class AdminModerationPageDto
{
    public List<AdminModerationWorkOrderDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}

/// <summary>
/// 举报综合处理请求
/// </summary>
public class HandleWorkOrderDto
{
    /// <summary>
    /// none/remove_product/ban_user/mute_user/restrict_publish/warn_user/restore_product/unban_user
    /// </summary>
    [Required(ErrorMessage = "处理动作不能为空")]
    public string Action { get; set; } = string.Empty;

    [Required(ErrorMessage = "处理原因不能为空")]
    [MaxLength(500, ErrorMessage = "处理原因最多500个字符")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 管理员回复申诉请求
/// </summary>
public class WorkOrderReplyDto
{
    [Required(ErrorMessage = "回复内容不能为空")]
    [MaxLength(500, ErrorMessage = "回复内容最多500个字符")]
    public string Reply { get; set; } = string.Empty;
}

/// <summary>
/// 待处理任务概览
/// </summary>
public class AdminModerationTasksDto
{
    public int TotalPending { get; set; }
    public int WaitingCount { get; set; }
    public int ProcessingCount { get; set; }
    public int ReportCount { get; set; }
    public int AppealCount { get; set; }
    public List<AdminModerationWorkOrderDto> RecentTasks { get; set; } = new();
}
