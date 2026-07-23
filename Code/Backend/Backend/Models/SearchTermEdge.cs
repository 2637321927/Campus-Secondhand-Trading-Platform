using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("search_term_edge")]
public class SearchTermEdge
{

    [Key]
    [Column("edge_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long EdgeId { get; set; }

    [Column("term1_id")]
    public long Term1Id { get; set; }

    [Column("term2_id")]
    public long Term2Id { get; set; }

    [Column("weight")]
    public double Weight { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey("Term1Id")]
    public SearchTerm? Term1 { get; set; }

    [ForeignKey("Term2Id")]
    public SearchTerm? Term2 { get; set; }
    
}
