using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("product_view")]
public class ProductView
{
    [Key]
    [Column("view_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long ViewId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("view_time")]
    public DateTime ViewTime { get; set; } = DateTime.Now;

    [ForeignKey("UserId")]
    public NormUser? User { get; set; }

    [ForeignKey("ProductId")]
    public Product? Product { get; set; }
}
