using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class PaymentConfig : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.PaymentId).UseIdentityColumn();

        builder.HasOne(p => p.Purchase)
            .WithMany(p => p.Payments)
            .HasForeignKey(p => p.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
