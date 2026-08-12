using Backend.Dtos.Payment;

namespace Backend.Services;

public interface IPaymentService
{
    /// <summary>
    /// 获取可用支付方式列表
    /// </summary>
    List<PaymentMethodDto> GetPaymentMethods();

    /// <summary>
    /// 发起支付
    /// </summary>
    Task<PaymentDto> CreatePaymentAsync(int userId, CreatePaymentDto dto);

    /// <summary>
    /// 查询支付状态
    /// </summary>
    Task<PaymentStatusDto> GetPaymentStatusAsync(long paymentId);

    /// <summary>
    /// 取消支付
    /// </summary>
    Task<PaymentDto> CancelPaymentAsync(long paymentId, int userId);

    /// <summary>
    /// 接收第三方支付回调（模拟）
    /// </summary>
    Task<PaymentDto> HandleCallbackAsync(long paymentId, PaymentCallbackDto dto);
}
