using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 系统通知表 — 系统向用户推送的消息
/// </summary>
[Table("sys_info")]
public class SysInfo
{
    [Key]
    [Column("sys_info_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SysInfoId { get; set; }

    [Column("detailed")]
    [MaxLength(200)]
    public string Detailed { get; set; } = string.Empty;

    [Column("release_time")]
    public DateTime ReleaseTime { get; set; } = DateTime.Now;

    [Column("user_id")]
    public int UserId { get; set; }

    // 导航属性
    [ForeignKey("UserId")]
    public NormUser? User { get; set; }
}
