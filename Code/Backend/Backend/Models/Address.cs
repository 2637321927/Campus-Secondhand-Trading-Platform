using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 收货地址表 — 用户的收货信息
/// </summary>
[Table("address")]
public class Address
{
    [Key]
    [Column("address_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AddressId { get; set; }

    [Column("name")]
    [MaxLength(10)]
    public string Name { get; set; } = string.Empty;

    [Column("phone_number")]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Column("detail_address")]
    [MaxLength(50)]
    public string DetailAddress { get; set; } = string.Empty;

    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// 是否默认地址：0=否，1=是
    /// </summary>
    [Column("is_default")]
    public int IsDefault { get; set; } = 0;

    // 导航属性
    [ForeignKey("UserId")]
    public NormUser? User { get; set; }
}
