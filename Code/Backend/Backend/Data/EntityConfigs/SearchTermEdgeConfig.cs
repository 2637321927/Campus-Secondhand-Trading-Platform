using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class SearchTermEdgeConfig : IEntityTypeConfiguration<SearchTermEdge>
{
    public void Configure(EntityTypeBuilder<SearchTermEdge> builder)
    {
        builder.Property(e => e.EdgeId).UseIdentityColumn();

        // 联合唯一索引保证(term1_id, term2_id)唯一，term1_id < term2_id
        builder.HasIndex(e => new { e.Term1Id, e.Term2Id }).IsUnique();

        builder.HasOne(e => e.Term1)
            .WithMany()
            .HasForeignKey(e => e.Term1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Term2)
            .WithMany()
            .HasForeignKey(e => e.Term2Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
