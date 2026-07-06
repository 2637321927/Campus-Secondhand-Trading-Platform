using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 退款审核表 — 退款申请的审核结果（联合主键：refund_id + reviewer_type）
/// </summary>
[Table("refund_review")]
public class RefundReview
{
    [Key]
    [Column("refund_id", Order = 0)]
    public long RefundId { get; set; }

    /// <summary>
    /// 审核者类型（联合主键）：0=卖家，1=管理员
    /// </summary>
    [Key]
    [Column("reviewer_type", Order = 1)]
    public int ReviewerType { get; set; }

    /// <summary>
    /// 审核结果：0=拒绝，1=通过
    /// </summary>
    [Column("result")]
    public int Result { get; set; }

    [Column("review_time")]
    public DateTime ReviewTime { get; set; } = DateTime.Now;

    [Column("info")]
    [MaxLength(500)]
    public string? Info { get; set; }

    // 导航属性
    [ForeignKey("RefundId")]
    public Refund? Refund { get; set; }
}
