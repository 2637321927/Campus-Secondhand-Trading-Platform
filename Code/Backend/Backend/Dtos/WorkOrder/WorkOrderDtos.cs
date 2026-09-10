using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.WorkOrder;

/// <summary>
/// 工单模块 DTO — 用户举报与申诉（统一工单 work_order）
/// </summary>

/// <summary>
/// 发起举报请求
/// </summary>
public class CreateReportDto
{
    /// <summary>举报对象类型（如 product/user）</summary>
    [Required, MaxLength(20)]
    public string TargetType { get; set; } = string.Empty;

    public long TargetId { get; set; }

    [Required, MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Info { get; set; }

    public int? AccusedId { get; set; }
    public long? ProductId { get; set; }
}

/// <summary>
/// 发起申诉请求
/// </summary>
public class CreateAppealDto
{
    [Required, MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Info { get; set; }

    /// <summary>申诉针对的工单 ID（如被处罚的举报单）</summary>
    public long? AppealAgainstId { get; set; }

    [MaxLength(20)]
    public string? TargetType { get; set; }

    public long? TargetId { get; set; }
}

/// <summary>
/// 申诉补充说明
/// </summary>
public class AppendWorkOrderMessageDto
{
    [Required, MaxLength(500)]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// 工单（举报/申诉）通用返回项
/// </summary>
public class WorkOrderDto
{
    public long Id { get; set; }

    /// <summary>工单类型：1=举报，2=申诉（对应 WorkOrderType 枚举）</summary>
    public int Type { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string? Info { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public string? Response { get; set; }
    public DateTime CreateTime { get; set; }
    public string? TargetType { get; set; }
    public long? TargetId { get; set; }
    public long? AppealAgainstId { get; set; }
}
