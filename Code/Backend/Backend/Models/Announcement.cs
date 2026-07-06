using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 公告表 — 管理员发布的平台公告
/// </summary>
[Table("announcement")]
public class Announcement
{
    [Key]
    [Column("announcement_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AnnouncementId { get; set; }

    [Column("title")]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Column("info")]
    [MaxLength(500)]
    public string Info { get; set; } = string.Empty;

    [Column("release_time")]
    public DateTime ReleaseTime { get; set; } = DateTime.Now;

    [Column("admin_id")]
    public int AdminId { get; set; }

    // 导航属性
    [ForeignKey("AdminId")]
    public AdminUser? Admin { get; set; }
}
