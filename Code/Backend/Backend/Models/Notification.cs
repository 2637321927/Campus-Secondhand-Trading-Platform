using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 系统通知表 — 面向单个用户的站内动态通知
/// 例如：商品审核通过/驳回、被下架/恢复、被购买、支付成功、已发货、已收货等。
/// </summary>
[Table("notification")]
public class Notification
{
    [Key]
    [Column("notification_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long NotificationId { get; set; }

    /// <summary>
    /// 接收通知的用户
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// 通知类型：product=商品动态，order=订单动态
    /// </summary>
    [Column("type")]
    [MaxLength(30)]
    public string Type { get; set; } = "system";

    [Column("title")]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 关联对象 ID（如商品 ID、订单 ID），无则 null
    /// </summary>
    [Column("related_id")]
    public long? RelatedId { get; set; }

    /// <summary>
    /// 是否已读：0=未读，1=已读
    /// </summary>
    [Column("is_read")]
    public int IsRead { get; set; } = 0;

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("UserId")]
    public BaseUser? User { get; set; }
}
