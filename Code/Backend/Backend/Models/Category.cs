using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 商品分类表 — 支持二级分类（自关联）
/// </summary>
[Table("category")]
public class Category
{
    [Key]
    [Column("category_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long CategoryId { get; set; }

    [Column("category_name")]
    [MaxLength(20)]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 父分类ID，null=一级分类，非null=二级分类
    /// </summary>
    [Column("parent_id")]
    public long? ParentId { get; set; }

    // 导航属性
    [ForeignKey("ParentId")]
    public Category? Parent { get; set; }

    public ICollection<Category> Children { get; set; } = new List<Category>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
