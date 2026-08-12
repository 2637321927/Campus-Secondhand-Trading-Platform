using Backend.Dtos.Payment;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IOrderTimelineRepository _timelineRepo;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IPurchaseRepository purchaseRepo,
        IOrderTimelineRepository timelineRepo)
    {
        _paymentRepo = paymentRepo;
        _purchaseRepo = purchaseRepo;
        _timelineRepo = timelineRepo;
    }

    public List<PaymentMethodDto> GetPaymentMethods()
    {
        return new List<PaymentMethodDto>
        {
            new() { Value = "alipay", Label = "支付宝" },
            new() { Value = "wechat", Label = "微信支付" },
            new() { Value = "cash", Label = "线下现金" },
            new() { Value = "other", Label = "其他方式" }
        };
    }

    public async Task<PaymentDto> CreatePaymentAsync(int userId, CreatePaymentDto dto)
    {
        var order = await _purchaseRepo.GetByIdAsync(dto.PurchaseId)
            ?? throw new ArgumentException("订单不存在");

        if (order.BuyerId != userId)
            throw new UnauthorizedAccessException("只有买家可以发起支付");

        if (order.Status != "pending")
            throw new InvalidOperationException("只有待付款状态的订单可以发起支付");

        // 检查是否有进行中的支付
        var pending = await _paymentRepo.GetPendingByPurchaseIdAsync(dto.PurchaseId);
        if (pending != null)
            throw new InvalidOperationException("该订单已有待处理的支付，请勿重复发起");

        var payment = new Payment
        {
            Status = PaymentStatus.Pending,
            PaymentMethod = dto.PaymentMethod,
            Amount = order.Product?.Price ?? 0 + order.ShippingFees,
            CreateTime = DateTime.Now,
            PurchaseId = dto.PurchaseId
        };

        await _paymentRepo.AddAsync(payment);
        await _paymentRepo.SaveAsync();

        return ToDto(payment);
    }

    public async Task<PaymentStatusDto> GetPaymentStatusAsync(long paymentId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new ArgumentException("支付记录不存在");

        return new PaymentStatusDto
        {
            PaymentId = payment.PaymentId,
            Status = payment.Status,
            Amount = payment.Amount,
            CreateTime = payment.CreateTime,
            PayTime = payment.PayTime
        };
    }

    public async Task<PaymentDto> CancelPaymentAsync(long paymentId, int userId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new ArgumentException("支付记录不存在");

        if (payment.Purchase == null || payment.Purchase.BuyerId != userId)
            throw new UnauthorizedAccessException("只有买家可以取消支付");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("只有待支付状态的支付可以取消");

        payment.Status = PaymentStatus.Cancelled;
        payment.CancelTime = DateTime.Now;
        _paymentRepo.Update(payment);
        await _paymentRepo.SaveAsync();

        return ToDto(payment);
    }

    public async Task<PaymentDto> HandleCallbackAsync(long paymentId, PaymentCallbackDto dto)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId)
            ?? throw new ArgumentException("支付记录不存在");

        if (payment.Status != PaymentStatus.Pending)
            throw new InvalidOperationException("该支付已处理，无法重复回调");

        if (dto.Result == "success")
        {
            payment.Status = PaymentStatus.Paid;
            payment.PayTime = DateTime.Now;
            payment.TransactionId = dto.TransactionId;

            // 更新订单状态
            if (payment.Purchase != null && payment.Purchase.Status == "pending")
            {
                var oldStatus = payment.Purchase.Status;
                payment.Purchase.Status = "paid";
                payment.Purchase.PayTime = DateTime.Now;
                _purchaseRepo.Update(payment.Purchase);

                await _timelineRepo.AddAsync(new OrderTimeline
                {
                    PurchaseId = payment.PurchaseId,
                    OldStatus = oldStatus,
                    NewStatus = "paid",
                    ChangeTime = DateTime.Now,
                    OperatorId = payment.Purchase.BuyerId,
                    Note = $"支付成功（{payment.PaymentMethod}），交易号：{dto.TransactionId}"
                });
                await _timelineRepo.SaveAsync();
            }
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
        }

        _paymentRepo.Update(payment);
        await _paymentRepo.SaveAsync();

        return ToDto(payment);
    }

    private static PaymentDto ToDto(Payment p) => new()
    {
        PaymentId = p.PaymentId,
        Status = p.Status,
        PaymentMethod = p.PaymentMethod,
        Amount = p.Amount,
        TransactionId = p.TransactionId,
        CreateTime = p.CreateTime,
        PayTime = p.PayTime,
        CancelTime = p.CancelTime,
        PurchaseId = p.PurchaseId
    };
}
