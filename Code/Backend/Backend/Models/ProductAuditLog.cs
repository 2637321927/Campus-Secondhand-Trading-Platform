using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.Enums;

namespace Backend.Models;

/// <summary>
/// 商品审核与管理日志表
/// </summary>
[Table("product_audit_log")]
public class ProductAuditLog
{
    [Key]
    [Column("audit_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long AuditId { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("admin_id")]
    public int AdminId { get; set; }

    /// <summary>
    /// 操作类型：approve/reject/remove/restore/delete
    /// </summary>
    [Column("action")]
    [MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    [Column("reason")]
    [MaxLength(500)]
    public string? Reason { get; set; }

    [Column("old_status")]
    public ProductStatus OldStatus { get; set; }

    [Column("new_status")]
    public ProductStatus NewStatus { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("AdminId")]
    public AdminUser? Admin { get; set; }
}
