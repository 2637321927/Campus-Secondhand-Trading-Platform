using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("product_comment")]
public class ProductComment
{

    [Key]
    [Column("comment_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long CommentId { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("content")]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    [Column("index")]
    public int Index { get; set; }

    [Column("create_time")]
    public DateTime CreateTime { get; set; } = DateTime.Now;

    [Column("ResponseToId")]
    public long? ResponseToId { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }

    [ForeignKey("UserId")]
    public NormUser? User { get; set; }

    [ForeignKey("ResponseToId")]
    public ProductComment? ResponseTo { get; set; }

    [InverseProperty("ResponseTo")]
    public ICollection<ProductComment> Replies { get; set; } = new List<ProductComment>();

}