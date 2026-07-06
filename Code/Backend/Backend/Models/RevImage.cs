using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 评价图片表 — 评价附带的图片
/// </summary>
[Table("rev_image")]
public class RevImage
{
    [Key]
    [Column("img_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ImgId { get; set; }

    [Column("img_url")]
    [MaxLength(100)]
    public string ImgUrl { get; set; } = string.Empty;

    /// <summary>
    /// 图片排序序号
    /// </summary>
    [Column("img_index")]
    public int ImgIndex { get; set; }

    [Column("review_id")]
    public int ReviewId { get; set; }

    // 导航属性
    [ForeignKey("ReviewId")]
    public Review? Review { get; set; }
}
