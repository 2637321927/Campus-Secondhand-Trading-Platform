using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Communication;

/// <summary>
/// 会话模块 DTO — 买家针对商品发起的一对一咨询会话
/// </summary>

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
