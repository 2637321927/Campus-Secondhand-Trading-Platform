using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.Enums;

namespace Backend.Models;

/// <summary>
/// 支付记录表 — 订单的支付流水
/// </summary>
[Table("payment")]
public class Payment
{
    [Key]
    [Column("payment_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PaymentId { get; set; }

    /// <summary>
    /// 支付状态：pending/paid/failed/cancelled/refunding/refunded
    /// </summary>
    [Column("status")]
    [MaxLength(20)]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// 支付方式：alipay/wechat/cash/other
    /// </summary>
    [Column("payment_method")]
    [MaxLength(20)]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Alipay;

    /// <summary>
    /// 支付金额
    /// </summary>
    [Column("amount", TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 第三方支付交易号
    /// </summary>
    [Column("transaction_id")]
    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [Column("pay_time")]
    public DateTime? PayTime { get; set; }

    [Column("cancel_time")]
    public DateTime? CancelTime { get; set; }

    /// <summary>
    /// 关联订单ID
    /// </summary>
    [Column("purchase_id")]
    public long PurchaseId { get; set; }

    // 导航属性
    [ForeignKey("PurchaseId")]
    public Purchase? Purchase { get; set; }
}
