namespace Backend.Models.Enums;

public enum PaymentStatus
{
    Pending,   // 待支付
    Paid,      // 已支付
    Failed,    // 支付失败
    Cancelled, // 已取消
    Refunding, // 退款中
    Refunded   // 已退款
}
