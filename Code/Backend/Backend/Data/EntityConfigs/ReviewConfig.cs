using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class ReviewConfig : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.ReviewId).UseIdentityColumn();

        // PurchaseId 唯一（一个订单只有一个评价）
        builder.HasIndex(r => r.PurchaseId).IsUnique();
    }
}
