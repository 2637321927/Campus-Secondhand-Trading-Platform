using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 订单状态流转记录表 — 记录订单每次状态变更的详情
/// </summary>
[Table("order_timeline")]
public class OrderTimeline
{
    [Key]
    [Column("timeline_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long TimelineId { get; set; }

    /// <summary>
    /// 变更前的状态
    /// </summary>
    [Column("old_status")]
    [MaxLength(20)]
    public string? OldStatus { get; set; }

    /// <summary>
    /// 变更后的状态
    /// </summary>
    [Column("new_status")]
    [MaxLength(20)]
    public string NewStatus { get; set; } = string.Empty;

    /// <summary>
    /// 变更时间
    /// </summary>
    [Column("change_time")]
    public DateTime ChangeTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 操作人ID（买家、卖家或管理员）
    /// </summary>
    [Column("operator_id")]
    public int OperatorId { get; set; }

    /// <summary>
    /// 备注说明
    /// </summary>
    [Column("note")]
    [MaxLength(200)]
    public string? Note { get; set; }

    [Column("purchase_id")]
    public long PurchaseId { get; set; }

    // 导航属性
    [ForeignKey("PurchaseId")]
    public Purchase? Purchase { get; set; }
}
