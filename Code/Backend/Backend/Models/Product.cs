using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [MaxLength(30)]
    public string Name { get; set; } = string.Empty;

    [Column("price", TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    [Column("info")]
    [MaxLength(100)]
    public string? Info { get; set; }

    [Column("release_date")]
    public DateTime ReleaseDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 商品状态：available/sold/removed
    /// </summary>
    [Column("status")]
    [MaxLength(10)]
    public string Status { get; set; } = "available";

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("category_id")]
    public long CategoryId { get; set; }

    // 导航属性
    [ForeignKey("UserId")]
    public NormUser? Seller { get; set; }

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    public ICollection<ProdImage> Images { get; set; } = new List<ProdImage>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
}
