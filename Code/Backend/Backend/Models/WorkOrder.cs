using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 工单表 — 用户举报或申诉的业务工单
/// </summary>
[Table("work_order")]
public class WorkOrder
{
    [Key]
    [Column("work_order_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long WorkOrderId { get; set; }

    /// <summary>
    /// 工单类型
    /// </summary>
    [Column("type")]
    public int Type { get; set; }

    [Column("reason")]
    [MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [Column("info")]
    [MaxLength(500)]
    public string? Info { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 工单状态：waiting/processing/done
    /// </summary>
    [Column("status")]
    [MaxLength(15)]
    public string Status { get; set; } = "waiting";

    [Column("response")]
    [MaxLength(500)]
    public string? Response { get; set; }

    [Column("response_time")]
    public DateTime? ResponseTime { get; set; }

    [Column("initiator_id")]
    public int InitiatorId { get; set; }

    [Column("accused_id")]
    public int? AccusedId { get; set; }

    [Column("product_id")]
    public long? ProductId { get; set; }

    [Column("admin_id")]
    public int? AdminId { get; set; }

    // 导航属性
    [ForeignKey("InitiatorId")]
    public NormUser? Initiator { get; set; }

    [ForeignKey("AccusedId")]
    public NormUser? Accused { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("AdminId")]
    public AdminUser? Admin { get; set; }
}
