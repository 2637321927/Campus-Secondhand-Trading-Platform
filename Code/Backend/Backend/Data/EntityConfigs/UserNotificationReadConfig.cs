using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class UserNotificationReadConfig : IEntityTypeConfiguration<UserNotificationRead>
{
    public void Configure(EntityTypeBuilder<UserNotificationRead> builder)
    {
        builder.Property(r => r.ReadId).UseIdentityColumn();
        builder.HasIndex(r => new
        {
            r.UserId,
            r.NotificationType,
            r.NotificationId
        }).IsUnique();
    }
}
