using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class PurchaseConfig : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.Property(p => p.PurchaseId).UseIdentityColumn();

        builder.HasOne(p => p.Buyer)
            .WithMany()
            .HasForeignKey(p => p.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Address)
            .WithMany()
            .HasForeignKey(p => p.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 → Review
        builder.HasOne(p => p.Review)
            .WithOne(r => r.Purchase)
            .HasForeignKey<Review>(r => r.PurchaseId);

        // 1:1 → Refund
        builder.HasOne(p => p.Refund)
            .WithOne(r => r.Purchase)
            .HasForeignKey<Refund>(r => r.PurchaseId);
    }
}
