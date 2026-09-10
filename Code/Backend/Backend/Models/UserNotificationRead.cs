using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 用户通知已读记录表 — 按用户记录公告/警告的已读状态
/// </summary>
[Table("user_notification_read")]
public class UserNotificationRead
{
    [Key]
    [Column("read_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ReadId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>通知类型：announcement=系统公告，warning=用户警告</summary>
    [Column("notification_type")]
    [MaxLength(20)]
    public string NotificationType { get; set; } = "announcement";

    [Column("notification_id")]
    public long NotificationId { get; set; }

    [Column("read_time")]
    public DateTime ReadTime { get; set; } = DateTime.Now;
}
