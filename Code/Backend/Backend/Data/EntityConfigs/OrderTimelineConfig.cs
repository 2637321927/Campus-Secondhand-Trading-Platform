using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class OrderTimelineConfig : IEntityTypeConfiguration<OrderTimeline>
{
    public void Configure(EntityTypeBuilder<OrderTimeline> builder)
    {
        builder.Property(t => t.TimelineId).UseIdentityColumn();

        builder.HasOne(t => t.Purchase)
            .WithMany(p => p.Timelines)
            .HasForeignKey(t => t.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
