using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class WorkOrderTimelineConfig : IEntityTypeConfiguration<WorkOrderTimeline>
{
    public void Configure(EntityTypeBuilder<WorkOrderTimeline> builder)
    {
        builder.Property(t => t.TimelineId).UseIdentityColumn();

        builder.HasOne(t => t.WorkOrder)
            .WithMany(w => w.Timelines)
            .HasForeignKey(t => t.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Admin)
            .WithMany()
            .HasForeignKey(t => t.AdminId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
