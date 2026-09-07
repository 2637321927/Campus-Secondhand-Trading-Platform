using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class SearchTermSimilarityConfig : IEntityTypeConfiguration<SearchTermSimilarity>
{
    public void Configure(EntityTypeBuilder<SearchTermSimilarity> builder)
    {
        builder.Property(x => x.SimilarityId).UseIdentityColumn();
        builder.HasIndex(x => new { x.SourceTermId, x.SimilarTermId }).IsUnique();
        builder.HasIndex(x => new { x.SourceTermId, x.Rank }).IsUnique();

        builder.HasOne(x => x.SourceTerm)
            .WithMany()
            .HasForeignKey(x => x.SourceTermId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SimilarTerm)
            .WithMany()
            .HasForeignKey(x => x.SimilarTermId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

