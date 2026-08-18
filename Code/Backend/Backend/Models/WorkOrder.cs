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

    /// <summary>
    /// 举报/申诉对象类型：product/user/comment/message/order
    /// </summary>
    [Column("target_type")]
    [MaxLength(20)]
    public string? TargetType { get; set; }

    /// <summary>
    /// 举报/申诉对象ID
    /// </summary>
    [Column("target_id")]
    public long? TargetId { get; set; }

    /// <summary>
    /// 申诉针对的原始工单ID（仅申诉类型使用）
    /// </summary>
    [Column("appeal_against_id")]
    public long? AppealAgainstWorkOrderId { get; set; }

    /// <summary>
    /// 处理结果：accepted/rejected/handled/approved
    /// </summary>
    [Column("result")]
    [MaxLength(20)]
    public string? Result { get; set; }

    /// <summary>
    /// 处理动作：remove_product/ban_user/mute_user/restrict_publish/warn_user/restore_product/unban_user
    /// </summary>
    [Column("handle_action")]
    [MaxLength(50)]
    public string? HandleAction { get; set; }

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

    [ForeignKey("AppealAgainstWorkOrderId")]
    public WorkOrder? AppealAgainst { get; set; }

    public ICollection<WorkOrderTimeline> Timelines { get; set; } = new List<WorkOrderTimeline>();
}
