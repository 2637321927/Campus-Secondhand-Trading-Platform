using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.Enums;

namespace Backend.Models;

/// <summary>
/// 商品表 — 用户发布的二手商品
/// </summary>
[Table("product")]
public class Product
{
    [Key]
    [Column("product_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ProductId { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column("info")]
    [MaxLength(1000)]
    public string? Info { get; set; }

    [Column("release_date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 商品状态：available/sold/removed
    /// </summary>
    [Column("status")]
    [MaxLength(10)]
    public ProductStatus Status { get; set; } = ProductStatus.Available;

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("category_id")]
    public long CategoryId { get; set; }

    /// <summary>
    /// 发货方式：0=包邮，1=按距离，2=固定邮费，3=无需邮寄
    /// </summary>
    [Column("shipping_type")]
    public ShippingType ShippingType { get; set; } = ShippingType.Free;

    /// <summary>
    /// 固定邮费金额（仅 ShippingType=Fixed 时有效）
    /// </summary>
    [Column("shipping_fee", TypeName = "decimal(5,2)")]
    public decimal? ShippingFee { get; set; }

    /// <summary>
    /// 是否支持买家自提：0=不支持，1=支持
    /// </summary>
    [Column("allow_pickup")]
    public int AllowPickup { get; set; } = 0;

    [Column("reject_reason")]
    [MaxLength(500)]
    public string? RejectReason { get; set; }

    [Column("reviewed_by")]
    public int? ReviewedByAdminId { get; set; }

    [Column("reviewed_at")]
    public DateTime? ReviewedAt { get; set; }

    // 导航属性
    [ForeignKey("UserId")]
    public NormUser? Seller { get; set; }

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    [ForeignKey("ReviewedByAdminId")]
    public AdminUser? ReviewedBy { get; set; }

    public ICollection<ProdImage> Images { get; set; } = new List<ProdImage>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
    public ICollection<ProductView> Views { get; set; } = new List<ProductView>();
    public ICollection<ProductComment> Comments { get; set; } = new List<ProductComment>();
}
