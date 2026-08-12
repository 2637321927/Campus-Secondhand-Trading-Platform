using Backend.Models.Enums;

namespace Backend.Dtos.Payment;

/// <summary>
/// 发起支付请求
/// </summary>
public class CreatePaymentDto
{
    /// <summary>
    /// 订单ID
    /// </summary>
    public long PurchaseId { get; set; }

    /// <summary>
    /// 支付方式：alipay/wechat/cash/other
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Alipay;
}

/// <summary>
/// 支付记录响应
/// </summary>
public class PaymentDto
{
    public long PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? PayTime { get; set; }
    public DateTime? CancelTime { get; set; }
    public long PurchaseId { get; set; }
}

/// <summary>
/// 支付状态查询响应
/// </summary>
public class PaymentStatusDto
{
    public long PaymentId { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusText => Status switch
    {
        PaymentStatus.Pending => "待支付",
        PaymentStatus.Paid => "已支付",
        PaymentStatus.Failed => "支付失败",
        PaymentStatus.Cancelled => "已取消",
        PaymentStatus.Refunding => "退款中",
        PaymentStatus.Refunded => "已退款",
        _ => "未知"
    };
    public decimal Amount { get; set; }
    public DateTime CreateTime { get; set; }
    public DateTime? PayTime { get; set; }
}

/// <summary>
/// 支付方式响应
/// </summary>
public class PaymentMethodDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// 第三方支付回调请求（模拟）
/// </summary>
public class PaymentCallbackDto
{
    /// <summary>
    /// 第三方交易号
    /// </summary>
    public string? TransactionId { get; set; }

    /// <summary>
    /// 回调结果：success/fail
    /// </summary>
    public string Result { get; set; } = "success";
}
