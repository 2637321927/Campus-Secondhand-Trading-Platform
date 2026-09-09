using Backend.Dtos.Admin;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AdminOrderService : IAdminOrderService
{
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IProductRepository _productRepo;
    private readonly IOrderTimelineRepository _timelineRepo;

    public AdminOrderService(
        IPurchaseRepository purchaseRepo,
        IProductRepository productRepo,
        IOrderTimelineRepository timelineRepo)
    {
        _purchaseRepo = purchaseRepo;
        _productRepo = productRepo;
        _timelineRepo = timelineRepo;
    }

    public async Task<AdminOrderPageDto> GetOrdersAsync(
        long? orderId,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, total) = await _purchaseRepo.GetAdminPageAsync(
            orderId, status, startDate, endDate, page, pageSize);

        return new AdminOrderPageDto
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminOrderStatisticsDto> GetStatisticsAsync()
    {
        var query = _purchaseRepo.Query();

        return new AdminOrderStatisticsDto
        {
            TotalOrders = await query.CountAsync(),
            PendingCount = await query.CountAsync(o => o.Status == "pending"),
            PaidCount = await query.CountAsync(o => o.Status == "paid"),
            ConfirmedCount = await query.CountAsync(o => o.Status == "confirmed"),
            ShippingCount = await query.CountAsync(o => o.Status == "shipping"),
            CompletedCount = await query.CountAsync(o => o.Status == "success"),
            CancelledCount = await query.CountAsync(o => o.Status == "cancel")
        };
    }

    public async Task<AdminOrderDto?> GetOrderAsync(long orderId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId);
        return order == null ? null : ToDto(order);
    }

    public async Task<AdminOrderDto?> CancelOrderAsync(long orderId, int adminId)
    {
        var order = await GetOrderForAdminAsync(orderId);
        if (order == null) return null;

        if (order.Status is not ("pending" or "paid" or "confirmed"))
            throw new InvalidOperationException("当前订单状态不能取消");

        var oldStatus = order.Status;
        order.Status = "cancel";
        order.CancelTime = DateTime.Now;

        if (order.Product != null)
        {
            order.Product.Status = ProductStatus.Available;
            _productRepo.Update(order.Product);
        }

        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "cancel",
            ChangeTime = DateTime.Now,
            OperatorId = adminId,
            Note = "管理员取消订单"
        });
        await _timelineRepo.SaveAsync();

        return ToDto(order);
    }

    public async Task<AdminOrderDto?> CompleteOrderAsync(long orderId, int adminId)
    {
        var order = await GetOrderForAdminAsync(orderId);
        if (order == null) return null;

        if (order.Status != "shipping")
            throw new InvalidOperationException("只有发货中的订单可以完成");

        var oldStatus = order.Status;
        order.Status = "success";
        order.CompleteTime = DateTime.Now;

        if (order.Product != null)
        {
            order.Product.Status = ProductStatus.Sold;
            _productRepo.Update(order.Product);
        }

        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "success",
            ChangeTime = DateTime.Now,
            OperatorId = adminId,
            Note = "管理员完成订单"
        });
        await _timelineRepo.SaveAsync();

        return ToDto(order);
    }

    private async Task<Purchase?> GetOrderForAdminAsync(long orderId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId);
        return order;
    }

    private static AdminOrderDto ToDto(Purchase p) => new()
    {
        OrderId = p.PurchaseId,
        Status = p.Status,
        CreateTime = p.CreateTime,
        PayTime = p.PayTime,
        ShippingTime = p.ShippingTime,
        CompleteTime = p.CompleteTime,
        ShippingFees = p.ShippingFees,
        ProductPrice = p.Product?.Price ?? 0,
        TotalAmount = (p.Product?.Price ?? 0) + p.ShippingFees,
        ProductCoverImageId = p.Product?.Images?
            .OrderBy(i => i.ImgIndex)
            .Select(i => (long?)i.ImgFileId)
            .FirstOrDefault(),
        ProductId = p.ProductId,
        ProductName = p.Product?.Name ?? "",
        BuyerId = p.BuyerId,
        BuyerName = p.Buyer?.UserName ?? "",
        SellerId = p.Product?.UserId ?? 0,
        SellerName = p.Product?.Seller?.UserName ?? "",
        ShippingAddress = p.ShippingAddress,
        ReceivingAddress = p.ReceivingAddress,
        TrackingNumber = p.TrackingNumber
    };
}
