using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 用户基础表 — 所有用户的公共信息
/// </summary>
[Table("base_user")]
public class BaseUser
{
    [Key]
    [Column("user_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }

    [Column("email")]
    [MaxLength(50)]
    public string Email { get; set; } = string.Empty;

    [Column("pw_hash")]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("phone_number")]
    [MaxLength(11)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 用户类型：0=普通用户，1=管理员
    /// </summary>
    [Column("user_type")]
    public int UserType { get; set; }

    [Column("avatar_file_id")]
    public long? AvatarFileId { get; set; }

    [Column("gender")]
    [MaxLength(10)]
    public string Gender { get; set; } = "unknown";

    /// <summary>
    /// 是否封禁：0=否，1=是
    /// </summary>
    [Column("is_banned")]
    public int IsBanned { get; set; } = 0;

    [Column("banned_until")]
    public DateTime? BannedUntil { get; set; }

    [Column("register_time")]
    public DateTime RegisterTime { get; set; } = DateTime.Now;

    // 导航属性
    public NormUser? NormUser { get; set; }
    public AdminUser? AdminUser { get; set; }

    [ForeignKey("AvatarFileId")]
    public UpdatedFile? AvatarFile { get; set; }

}
