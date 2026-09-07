using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class UserWarningConfig : IEntityTypeConfiguration<UserWarning>
{
    public void Configure(EntityTypeBuilder<UserWarning> builder)
    {
        builder.Property(w => w.WarningId).UseIdentityColumn();

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Admin)
            .WithMany()
            .HasForeignKey(w => w.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
