using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

/// <summary>
/// 会话表 — 买家针对商品发起的咨询会话
/// </summary>
[Table("conversation")]
public class Conversation
{
    [Key]
    [Column("session_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SessionId { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("buyer_id")]
    public int BuyerId { get; set; }

    // 导航属性
    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("BuyerId")]
    public NormUser? Buyer { get; set; }

    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
