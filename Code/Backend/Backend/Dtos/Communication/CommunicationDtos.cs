using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Communication;

// ==================== 会话 ====================

/// <summary>
/// 创建会话请求（前端只传商品 ID，买家为当前登录用户）
/// </summary>
public class CreateConversationDto
{
    [Required]
    public long ProductId { get; set; }
}

/// <summary>
/// 发送消息请求：文字消息传 Content；带图消息走附件接口，FileId 由后端上传后填充
/// </summary>
public class SendMessageDto
{
    [MaxLength(1000)]
    public string? Content { get; set; }

    public long? FileId { get; set; }
}

/// <summary>
/// 会话列表/详情项
/// </summary>
public class ConversationDto
{
    public int ConversationId { get; set; }
    public long ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int BuyerId { get; set; }
    public int SellerId { get; set; }
    public DateTime CreateTime { get; set; }
    public int UnreadCount { get; set; }
}

/// <summary>
/// 单条聊天消息
/// </summary>
public class MessageDto
{
    public int ConversationId { get; set; }
    public int MessageId { get; set; }
    public int SenderId { get; set; }

    /// <summary>
    /// 消息类型：0=文字，1=图片（对应 MessageType 枚举）
    /// </summary>
    public int MessageType { get; set; }

    public long? FileId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SendTime { get; set; }
    public bool IsRead { get; set; }
}

// ==================== 工单（举报/申诉） ====================

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

    /// <summary>工单类型：0=举报，1=申诉（对应 WorkOrderType 枚举）</summary>
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

// ==================== 通知 ====================

/// <summary>
/// 站内通知项（基于公告表）
/// </summary>
public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public bool IsRead { get; set; }
}