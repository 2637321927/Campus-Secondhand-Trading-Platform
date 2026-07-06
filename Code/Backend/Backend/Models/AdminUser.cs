using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 管理员表 — 管理员扩展信息，1:1 关联 base_user
/// </summary>
[Table("admin_user")]
public class AdminUser
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// 权限等级：1-5
    /// </summary>
    [Column("permission")]
    public int Permission { get; set; } = 1;

    // 导航属性
    [ForeignKey("UserId")]
    public BaseUser? BaseUser { get; set; }

    public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}
