using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("search_term_similarity")]
public class SearchTermSimilarity
{
    [Key]
    [Column("similarity_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long SimilarityId { get; set; }

    [Column("source_term_id")]
    public long SourceTermId { get; set; }

    [Column("similar_term_id")]
    public long SimilarTermId { get; set; }

    [Column("similarity")]
    public double Similarity { get; set; }

    [Column("rank")]
    public int Rank { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(SourceTermId))]
    public SearchTerm? SourceTerm { get; set; }

    [ForeignKey(nameof(SimilarTermId))]
    public SearchTerm? SimilarTerm { get; set; }
}
