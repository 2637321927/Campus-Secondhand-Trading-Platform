using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

[Table("search_term")]
public class SearchTerm
{

    [Key]
    [Column("term_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long TermId { get; set; }

    [Column("term_text")]
    [MaxLength(100)]
    public string TermText { get; set; } = string.Empty;

    [Column("row_sum")]
    public double RowSum { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    
}
