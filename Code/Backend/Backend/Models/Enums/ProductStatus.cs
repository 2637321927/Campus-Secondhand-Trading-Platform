namespace Backend.Models.Enums;

public enum ProductStatus
{
    Available, // 在售
    Sold,      // 已售
    Removed,   // 已下架
    PendingReview, // 待审核
    Rejected,  // 审核驳回
    Reserved,  // 交易中（已被下单锁定）
    TakenDown  // 管理员强制下架（卖家不能自行重新上架）
}
