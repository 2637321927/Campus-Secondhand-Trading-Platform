using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 普通用户表 — 卖家/买家的扩展信息，1:1 关联 base_user
/// </summary>
[Table("norm_user")]
public class NormUser
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("user_name")]
    [MaxLength(20)]
    public string UserName { get; set; } = string.Empty;

    [Column("credit")]
    public int Credit { get; set; } = 100;

    [Column("profile")]
    [MaxLength(20)]
    public string? Profile { get; set; }

    // 导航属性
    [ForeignKey("UserId")]
    public BaseUser? BaseUser { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Collection> Collections { get; set; } = new List<Collection>();
    public ICollection<ProductView> Views { get; set; } = new List<ProductView>();
}
