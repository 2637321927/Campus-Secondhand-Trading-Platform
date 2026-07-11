using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class UpdatedFileConfig : IEntityTypeConfiguration<UpdatedFile>
{

    public void Configure(EntityTypeBuilder<UpdatedFile> builder)
    {

        builder.Property(f => f.FileId).UseIdentityColumn();

        builder.HasIndex(f => f.FileHash);

    }

}