using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class SearchTermConfig : IEntityTypeConfiguration<SearchTerm>
{
    public void Configure(EntityTypeBuilder<SearchTerm> builder)
    {
        builder.Property(t => t.TermId).UseIdentityColumn();

        builder.HasIndex(t => t.TermText).IsUnique();
    }
}
