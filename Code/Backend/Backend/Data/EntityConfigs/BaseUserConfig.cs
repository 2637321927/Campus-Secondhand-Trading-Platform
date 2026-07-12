using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class BaseUserConfig : IEntityTypeConfiguration<BaseUser>
{
    public void Configure(EntityTypeBuilder<BaseUser> builder)
    {
        // 自增主键（Oracle 序列）
        builder.Property(b => b.UserId).UseIdentityColumn();

        // 唯一约束
        builder.HasIndex(b => b.Email).IsUnique();
        builder.HasIndex(b => b.PhoneNumber).IsUnique();

        // 1:1 关系
        builder.HasOne(b => b.NormUser)
            .WithOne(n => n.BaseUser)
            .HasForeignKey<NormUser>(n => n.UserId);

        builder.HasOne(b => b.AdminUser)
            .WithOne(a => a.BaseUser)
            .HasForeignKey<AdminUser>(a => a.UserId);

        builder.HasOne(b => b.AvatarFile)
            .WithMany()
            .HasForeignKey(b => b.AvatarFileId);
    }
}
