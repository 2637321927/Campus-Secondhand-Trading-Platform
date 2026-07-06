using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 评价表 — 订单完成后的用户评价（一个订单对应一条评价）
/// </summary>
[Table("review")]
public class Review
{
    [Key]
    [Column("review_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ReviewId { get; set; }

    /// <summary>
    /// 评分：1-5
    /// </summary>
    [Column("rating")]
    public int Rating { get; set; }

    [Column("info")]
    [MaxLength(100)]
    public string? Info { get; set; }

    [Column("review_time")]
    public DateTime ReviewTime { get; set; } = DateTime.Now;

    [Column("purchase_id")]
    public long PurchaseId { get; set; }

    // 导航属性
    [ForeignKey("PurchaseId")]
    public Purchase? Purchase { get; set; }

    public ICollection<RevImage> Images { get; set; } = new List<RevImage>();
}
