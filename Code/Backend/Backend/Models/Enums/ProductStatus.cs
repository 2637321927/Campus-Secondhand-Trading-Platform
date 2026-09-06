namespace Backend.Models.Enums;

public enum ProductStatus
{
    Available, // 在售
    Sold,      // 已售
    Removed,   // 已下架
    PendingReview, // 待审核
    Rejected   // 审核驳回
}
