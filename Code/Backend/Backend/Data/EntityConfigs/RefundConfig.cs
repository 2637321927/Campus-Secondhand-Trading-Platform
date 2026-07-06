using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class RefundConfig : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.Property(r => r.RefundId).UseIdentityColumn();

        // PurchaseId 唯一（一个订单只有一个退款申请）
        builder.HasIndex(r => r.PurchaseId).IsUnique();
    }
}
