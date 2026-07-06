using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class NormUserConfig : IEntityTypeConfiguration<NormUser>
{
    public void Configure(EntityTypeBuilder<NormUser> builder)
    {
        // 不自增（FK 即 PK）
        builder.Property(n => n.UserId).ValueGeneratedNever();
    }
}
