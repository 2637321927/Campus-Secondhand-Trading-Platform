using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class ProductCommentConfig : IEntityTypeConfiguration<ProductComment>
{
    public void Configure(EntityTypeBuilder<ProductComment> builder)
    {
        
        builder.HasOne(c => c.Product)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 自引用：评论的回复关系
        builder.HasOne(c => c.ResponseTo)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ResponseToId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.ProductId, c.Index }).IsUnique();
        builder.HasIndex(c => c.ResponseToId);

    }
}
