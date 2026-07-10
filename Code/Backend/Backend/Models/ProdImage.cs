using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 商品图片表 — 商品的展示图片，一商品多图
/// </summary>
[Table("prod_image")]
public class ProdImage
{

    [Key]
    [Column("img_file_id")]
    [MaxLength(100)]
    public string ImgFileId { get; set; } = string.Empty;

    /// <summary>
    /// 图片排序序号，允许多张图片
    /// </summary>
    [Column("img_index")]
    public int ImgIndex { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    // 导航属性
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("ImgFileId")]
    public File? ImgFile { get; set; }

}
