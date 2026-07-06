using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 订单表 — 用户购买商品的交易单据
/// </summary>
[Table("purchase")]
public class Purchase
{
    [Key]
    [Column("purchase_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long PurchaseId { get; set; }

    /// <summary>
    /// 订单状态：pending/paid/shipping/success/cancel/refund
    /// </summary>
    [Column("status")]
    [MaxLength(10)]
    public string Status { get; set; } = "pending";

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [Column("cancel_time")]
    public DateTime? CancelTime { get; set; }

    [Column("pay_time")]
    public DateTime? PayTime { get; set; }

    [Column("shipping_time")]
    public DateTime? ShippingTime { get; set; }

    [Column("delivery_time")]
    public DateTime? DeliveryTime { get; set; }

    [Column("complete_time")]
    public DateTime? CompleteTime { get; set; }

    [Column("shipping_fees", TypeName = "decimal(5,2)")]
    public decimal ShippingFees { get; set; }

    /// <summary>
    /// 运费承担方：0=卖家付，1=买家付
    /// </summary>
    [Column("responsible_for_ship")]
    public int ResponsibleForShip { get; set; }

    [Column("buyer_id")]
    public int BuyerId { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("address_id")]
    public int AddressId { get; set; }

    // 导航属性
    [ForeignKey("BuyerId")]
    public NormUser? Buyer { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("AddressId")]
    public Address? Address { get; set; }

    public Review? Review { get; set; }
    public Refund? Refund { get; set; }
}
