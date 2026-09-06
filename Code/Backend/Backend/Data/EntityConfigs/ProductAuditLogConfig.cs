using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class ProductAuditLogConfig : IEntityTypeConfiguration<ProductAuditLog>
{
    public void Configure(EntityTypeBuilder<ProductAuditLog> builder)
    {
        builder.Property(x => x.AuditId).UseIdentityColumn();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Admin)
            .WithMany()
            .HasForeignKey(x => x.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
