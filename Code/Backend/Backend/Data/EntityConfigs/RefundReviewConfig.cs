using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class RefundReviewConfig : IEntityTypeConfiguration<RefundReview>
{
    public void Configure(EntityTypeBuilder<RefundReview> builder)
    {
        // 联合主键：refund_id + reviewer_type
        builder.HasKey(r => new { r.RefundId, r.ReviewerType });

        builder.HasOne(r => r.Refund)
            .WithMany(r => r.RefundReviews)
            .HasForeignKey(r => r.RefundId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
