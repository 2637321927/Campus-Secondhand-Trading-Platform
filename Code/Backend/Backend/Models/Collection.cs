using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 商品收藏表 — 用户收藏商品的关系（联合主键：product_id + user_id）
/// </summary>
[Table("collection")]
public class Collection
{
    [Key]
    [Column("product_id", Order = 0)]
    public long ProductId { get; set; }

    [Key]
    [Column("user_id", Order = 1)]
    public int UserId { get; set; }

    [Column("collection_time")]
    public DateTime CollectionTime { get; set; } = DateTime.Now;

    // 导航属性
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("UserId")]
    public NormUser? User { get; set; }
}
