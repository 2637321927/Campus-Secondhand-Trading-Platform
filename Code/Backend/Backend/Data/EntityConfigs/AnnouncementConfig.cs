using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class AnnouncementConfig : IEntityTypeConfiguration<Announcement>
{
    public void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.Property(a => a.AnnouncementId).UseIdentityColumn();
        builder.Property(a => a.IsPinned).HasDefaultValue(false);
        builder.Property(a => a.Status).HasDefaultValue("published");

        builder.HasOne(a => a.Admin)
            .WithMany(a => a.Announcements)
            .HasForeignKey(a => a.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
