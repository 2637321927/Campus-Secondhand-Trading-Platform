using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class RevImageConfig : IEntityTypeConfiguration<RevImage>
{
    public void Configure(EntityTypeBuilder<RevImage> builder)
    {
        builder.HasOne(r => r.Review)
            .WithMany(r => r.Images)
            .HasForeignKey(r => r.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
