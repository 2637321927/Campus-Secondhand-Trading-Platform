namespace Backend.Models.Enums;

public enum OrderStatus
{
    Pending,   // 待付款
    Paid,      // 已付款
    Shipping,  // 已发货
    Success,   // 已完成
    Cancel,    // 已取消
    Refund     // 退款中
}
