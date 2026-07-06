using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class WorkOrderConfig : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.Property(w => w.WorkOrderId).UseIdentityColumn();

        builder.HasOne(w => w.Initiator)
            .WithMany()
            .HasForeignKey(w => w.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Accused)
            .WithMany()
            .HasForeignKey(w => w.AccusedId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.Admin)
            .WithMany()
            .HasForeignKey(w => w.AdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
