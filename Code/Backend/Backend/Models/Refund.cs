using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 退款表 — 用户发起的退款申请（一个订单对应一条退款）
/// </summary>
[Table("refund")]
public class Refund
{
    [Key]
    [Column("refund_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long RefundId { get; set; }

    [Column("reason")]
    [MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [Column("shipping_fees", TypeName = "decimal(5,2)")]
    public decimal ShippingFees { get; set; }

    /// <summary>
    /// 运费承担方：0=卖家付，1=买家付
    /// </summary>
    [Column("responsible_for_ship")]
    public int ResponsibleForShip { get; set; }

    [Column("apply_time")]
    public DateTime ApplyTime { get; set; } = DateTime.Now;

    [Column("purchase_id")]
    public long PurchaseId { get; set; }

    // 导航属性
    [ForeignKey("PurchaseId")]
    public Purchase? Purchase { get; set; }

    public ICollection<RefundReview> RefundReviews { get; set; } = new List<RefundReview>();
}
