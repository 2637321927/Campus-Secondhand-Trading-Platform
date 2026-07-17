using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data;

/// <summary>
/// EF Core 数据库上下文 — 连接 Oracle 数据库的核心类
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ==================== DbSet 声明 ====================
    public DbSet<BaseUser> BaseUsers => Set<BaseUser>();
    public DbSet<NormUser> NormUsers => Set<NormUser>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProdImage> ProdImages => Set<ProdImage>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RevImage> RevImages => Set<RevImage>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<RefundReview> RefundReviews => Set<RefundReview>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<SysInfo> SysInfos => Set<SysInfo>();
    public DbSet<UpdatedFile> UpdatedFiles => Set<UpdatedFile>();
    public DbSet<ProductView> ProductViews => Set<ProductView>();
    public DbSet<ProductComment> ProductComments => Set<ProductComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 全局配置：所有自增主键使用 Oracle 序列
        modelBuilder.UseIdentityColumns();

        // 加载独立的 EntityConfigs
        modelBuilder.ApplyConfiguration(new EntityConfigs.BaseUserConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.NormUserConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.AdminUserConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.AddressConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.CategoryConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.ProductConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.ProdImageConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.CollectionConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.PurchaseConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.ReviewConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.RevImageConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.RefundConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.RefundReviewConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.ConversationConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.MessageConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.WorkOrderConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.AnnouncementConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.SysInfoConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.UpdatedFileConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.ProductViewConfig());
        modelBuilder.ApplyConfiguration(new EntityConfigs.ProductCommentConfig());
    }
}
