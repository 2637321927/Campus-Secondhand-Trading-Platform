using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class ProdImageConfig : IEntityTypeConfiguration<ProdImage>
{
    public void Configure(EntityTypeBuilder<ProdImage> builder)
    {
        builder.HasOne(p => p.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
