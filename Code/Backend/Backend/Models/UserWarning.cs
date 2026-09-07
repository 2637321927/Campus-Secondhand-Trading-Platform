using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 用户警告记录表 — 管理员向用户发送的警告
/// </summary>
[Table("user_warning")]
public class UserWarning
{
    [Key]
    [Column("warning_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long WarningId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("admin_id")]
    public int AdminId { get; set; }

    [Column("reason")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public NormUser? User { get; set; }

    [ForeignKey("AdminId")]
    public AdminUser? Admin { get; set; }
}
