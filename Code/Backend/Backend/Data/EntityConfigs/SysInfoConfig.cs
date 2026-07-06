using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class SysInfoConfig : IEntityTypeConfiguration<SysInfo>
{
    public void Configure(EntityTypeBuilder<SysInfo> builder)
    {
        builder.Property(s => s.SysInfoId).UseIdentityColumn();

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
