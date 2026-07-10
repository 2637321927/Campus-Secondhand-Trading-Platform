using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 消息表 — 会话内的聊天消息（联合主键：session_id + msg_index）
/// </summary>
[Table("message")]
public class Message
{
    [Key]
    [Column("session_id", Order = 0)]
    public int SessionId { get; set; }

    [Key]
    [Column("msg_index", Order = 1)]
    public int MsgIndex { get; set; }

    [Column("sender_id")]
    public int SenderId { get; set; }

    /// <summary>
    /// 消息类型：0=文本，1=文件
    /// </summary>
    [Column("msg_type")]
    public int MsgType { get; set; }

    [Column("file_id")]
    public long? FileId { get; set; } = null;

    [Column("msg_content")]
    [MaxLength(1000)]
    public string MsgContent { get; set; } = string.Empty;

    [Column("send_time")]
    public DateTime SendTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否已读：0=未读，1=已读
    /// </summary>
    [Column("is_read")]
    public int IsRead { get; set; } = 0;

    // 导航属性
    [ForeignKey("SessionId")]
    public Conversation? Conversation { get; set; }

    [ForeignKey("SenderId")]
    public NormUser? Sender { get; set; }

    [ForeignKey("FileId")]
    public File? File { get; set; }

}
