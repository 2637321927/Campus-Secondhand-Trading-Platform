using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class ProductViewConfig : IEntityTypeConfiguration<ProductView>
{
    public void Configure(EntityTypeBuilder<ProductView> builder)
    {
        builder.HasOne(v => v.Product)
            .WithMany(p => p.Views)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany(u => u.Views)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.ProductId);
    }
}
