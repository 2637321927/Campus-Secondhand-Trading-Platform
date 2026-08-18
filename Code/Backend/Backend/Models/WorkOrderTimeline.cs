using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 工单处理时间线 — 记录举报/申诉的每次处理动作
/// </summary>
[Table("work_order_timeline")]
public class WorkOrderTimeline
{
    [Key]
    [Column("timeline_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long TimelineId { get; set; }

    [Column("work_order_id")]
    public long WorkOrderId { get; set; }

    /// <summary>
    /// 动作：accept/reject/handle/approve/reply
    /// </summary>
    [Column("action")]
    [MaxLength(50)]
    public string Action { get; set; } = string.Empty;

    [Column("note")]
    [MaxLength(500)]
    public string? Note { get; set; }

    [Column("admin_id")]
    public int? AdminId { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [ForeignKey("WorkOrderId")]
    public WorkOrder? WorkOrder { get; set; }

    [ForeignKey("AdminId")]
    public AdminUser? Admin { get; set; }
}
