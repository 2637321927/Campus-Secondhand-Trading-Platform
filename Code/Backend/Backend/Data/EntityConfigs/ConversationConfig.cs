using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.EntityConfigs;

public class ConversationConfig : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.Property(c => c.SessionId).UseIdentityColumn();

        builder.HasOne(c => c.Product)
            .WithMany()
            .HasForeignKey(c => c.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Buyer)
            .WithMany()
            .HasForeignKey(c => c.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
