using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Communication;

public class CreateConversationDto { [Required] public long ProductId { get; set; } }
public class SendMessageDto { [MaxLength(1000)] public string? Content { get; set; } public long? FileId { get; set; } }
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
public class MessageDto
{
    public int ConversationId { get; set; }
    public int MessageId { get; set; }
    public int SenderId { get; set; }
    public int MessageType { get; set; }
    public long? FileId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SendTime { get; set; }
    public bool IsRead { get; set; }
}

public class CreateReportDto
{
    [Required, MaxLength(20)] public string TargetType { get; set; } = string.Empty;
    public long TargetId { get; set; }
    [Required, MaxLength(100)] public string Reason { get; set; } = string.Empty;
    [MaxLength(500)] public string? Info { get; set; }
    public int? AccusedId { get; set; }
    public long? ProductId { get; set; }
}
public class CreateAppealDto
{
    [Required, MaxLength(100)] public string Reason { get; set; } = string.Empty;
    [MaxLength(500)] public string? Info { get; set; }
    public long? AppealAgainstId { get; set; }
    [MaxLength(20)] public string? TargetType { get; set; }
    public long? TargetId { get; set; }
}
public class AppendWorkOrderMessageDto { [Required, MaxLength(500)] public string Message { get; set; } = string.Empty; }
public class WorkOrderDto
{
    public long Id { get; set; }
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
public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public bool IsRead { get; set; }
}
