using Backend.Dtos.Order;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

public class OrderService : IOrderService
{
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IProductRepository _productRepo;
    private readonly IAddressRepository _addressRepo;
    private readonly IOrderTimelineRepository _timelineRepo;
    private readonly IReputationService _reputationService;
    private readonly IPaymentRepository _paymentRepo;
    private readonly INotificationService _notifications;
    private readonly int _expireMinutes;

    public OrderService(
        IPurchaseRepository purchaseRepo,
        IProductRepository productRepo,
        IAddressRepository addressRepo,
        IOrderTimelineRepository timelineRepo,
        IReputationService reputationService,
        IPaymentRepository paymentRepo,
        INotificationService notifications,
        IConfiguration configuration)
    {
        _purchaseRepo = purchaseRepo;
        _productRepo = productRepo;
        _addressRepo = addressRepo;
        _timelineRepo = timelineRepo;
        _reputationService = reputationService;
        _paymentRepo = paymentRepo;
        _notifications = notifications;
        _expireMinutes = configuration.GetValue("OrderAutoCancel:ExpireMinutes", 5);
    }

    public async Task<PurchaseCheckDto> PurchaseCheckAsync(long productId, int userId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
            return new PurchaseCheckDto { CanPurchase = false, Reason = "商品不存在" };

        var isOwn = product.UserId == userId;
        var canPurchase = product.Status == ProductStatus.Available && !isOwn;

        // 检查是否有进行中的订单（防止重复下单）
        var existingOrders = await _purchaseRepo.GetByProductIdAsync(productId);
        var hasActiveOrder = existingOrders.Any(o =>
            o.BuyerId == userId && o.Status != "cancel" && o.Status != "success");

        string? reason = null;
        if (product.Status != ProductStatus.Available)
            reason = "商品当前不可购买（已售出、交易中或已下架）";
        else if (isOwn)
            reason = "不能购买自己发布的商品";
        else if (hasActiveOrder)
            reason = "您已有该商品的进行中订单";

        return new PurchaseCheckDto
        {
            CanPurchase = canPurchase && !hasActiveOrder,
            Reason = reason,
            ProductStatus = product.Status.ToString().ToLower(),
            IsOwnProduct = isOwn,
            ShippingType = product.ShippingType
        };
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto)
    {
        var product = await _productRepo.GetByIdAsync(dto.ProductId);
        if (product == null)
            throw new ArgumentException("商品不存在");

        if (product.Status != ProductStatus.Available)
            throw new InvalidOperationException("商品当前不可购买");

        if (product.UserId == userId)
            throw new InvalidOperationException("不能购买自己发布的商品");

        var isPickup = dto.IsPickup && product.AllowPickup == 1;

        if (!isPickup)
        {
            var address = await _addressRepo.GetByIdAsync(dto.AddressId);
            if (address == null || address.UserId != userId)
                throw new ArgumentException("收货地址无效");
        }

        // 检查是否有进行中的订单
        var existingOrders = await _purchaseRepo.GetByProductIdAsync(dto.ProductId);
        if (existingOrders.Any(o => o.BuyerId == userId && o.Status != "cancel" && o.Status != "success"))
            throw new InvalidOperationException("您已有该商品的进行中订单");

        var order = new Purchase
        {
            Status = "pending",
            CreateTime = DateTime.Now,
            BuyerId = userId,
            ProductId = dto.ProductId,
            AddressId = isPickup ? null : dto.AddressId,
            IsPickup = isPickup ? 1 : 0,
            ShippingMethod = isPickup ? "自提" : dto.ShippingMethod,
            ShippingFees = isPickup ? 0 : (product.ShippingFee ?? 0),
            ResponsibleForShip = isPickup ? 0 : (product.ShippingType == ShippingType.Free ? 0 : 1)
        };

        // 下单即锁定商品，防止他人重复购买（交易中）
        product.Status = ProductStatus.Reserved;
        _productRepo.Update(product);

        await _purchaseRepo.AddAsync(order);
        await _purchaseRepo.SaveAsync();

        // 记录时间线
        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = order.PurchaseId,
            OldStatus = null,
            NewStatus = "pending",
            ChangeTime = DateTime.Now,
            OperatorId = userId,
            Note = isPickup ? "创建自提订单" : "创建订单"
        });
        await _timelineRepo.SaveAsync();

        // 通知卖家商品被购买
        await _notifications.NotifyAsync(
            product.UserId,
            "商品被购买",
            $"您的商品《{product.Name}》已被下单购买，请及时处理",
            "order",
            order.PurchaseId);

        return ToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(long orderId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId);
        return order == null ? null : ToDto(order);
    }

    public async Task<List<OrderListItemDto>> GetBuyingOrdersAsync(int userId)
    {
        var orders = await _purchaseRepo.GetByBuyerIdAsync(userId);
        return orders.Select(ToListItem).ToList();
    }

    public async Task<List<OrderListItemDto>> GetSellingOrdersAsync(int userId)
    {
        var orders = await _purchaseRepo.GetBySellerIdAsync(userId);
        return orders.Select(ToListItem).ToList();
    }

    public async Task<OrderDto> CancelOrderAsync(long orderId, int userId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        if (order.BuyerId != userId)
            throw new UnauthorizedAccessException("只有买家可以取消订单");

        if (order.Status != "pending")
            throw new InvalidOperationException("只有待付款状态的订单可以取消");

        var oldStatus = order.Status;
        order.Status = "cancel";
        order.CancelTime = DateTime.Now;

        // 取消订单后恢复商品在售
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
            OperatorId = userId,
            Note = "买家取消订单"
        });
        await _timelineRepo.SaveAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> SellerConfirmAsync(long orderId, int userId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        if (order.Product == null || order.Product.UserId != userId)
            throw new UnauthorizedAccessException("只有卖家可以确认订单");

        if (order.Status != "paid")
            throw new InvalidOperationException("只有已付款状态的订单可以确认");

        var oldStatus = order.Status;

        // 自提订单：卖家确认后直接完成，无需发货/收货环节
        if (order.IsPickup == 1)
        {
            order.Status = "success";
            order.DeliveryTime = DateTime.Now;
            order.CompleteTime = DateTime.Now;

            if (order.Product != null)
            {
                order.Product.Status = ProductStatus.Sold;
                _productRepo.Update(order.Product);
            }

            _purchaseRepo.Update(order);
            await _purchaseRepo.SaveAsync();

            if (order.Product != null)
                await _reputationService.ChangeCreditAsync(order.Product.UserId, CreditRules.OrderCompleted);

            await _timelineRepo.AddAsync(new OrderTimeline
            {
                PurchaseId = orderId,
                OldStatus = oldStatus,
                NewStatus = "success",
                ChangeTime = DateTime.Now,
                OperatorId = userId,
                Note = "卖家确认自提订单，订单完成"
            });
            await _timelineRepo.SaveAsync();

            // 通知双方
            await _notifications.NotifyAsync(
                order.BuyerId,
                "自提订单已完成",
                $"您的自提订单《{order.Product?.Name}》已完成，请及时与卖家约定取货",
                "order",
                orderId);
            if (order.Product != null)
                await _notifications.NotifyAsync(
                    order.Product.UserId,
                    "自提订单已完成",
                    $"您的商品《{order.Product.Name}》自提订单已完成",
                    "order",
                    orderId);

            return ToDto(order);
        }

        order.Status = "confirmed";
        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "confirmed",
            ChangeTime = DateTime.Now,
            OperatorId = userId,
            Note = "卖家确认订单"
        });
        await _timelineRepo.SaveAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> SellerRejectAsync(long orderId, int userId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        if (order.Product == null || order.Product.UserId != userId)
            throw new UnauthorizedAccessException("只有卖家可以拒绝订单");

        if (order.Status != "paid")
            throw new InvalidOperationException("只有已付款状态的订单可以拒绝");

        var oldStatus = order.Status;
        order.Status = "cancel";
        order.CancelTime = DateTime.Now;

        // 拒绝已付款订单：恢复商品在售（退款另见后续处理）
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
            OperatorId = userId,
            Note = "卖家拒绝订单"
        });
        await _timelineRepo.SaveAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> UpdateShippingAsync(long orderId, int userId, UpdateShippingDto dto)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        // 买家或卖家都可以修改（在发货前）
        if (order.BuyerId != userId && (order.Product == null || order.Product.UserId != userId))
            throw new UnauthorizedAccessException("无权修改该订单的配送信息");

        if (order.Status == "confirmed" || order.Status == "shipping" || order.Status == "success" || order.Status == "cancel")
            throw new InvalidOperationException("当前订单状态不允许修改配送信息");

        if (dto.ShippingMethod != null)
            order.ShippingMethod = dto.ShippingMethod;
        if (dto.ShippingAddress != null)
            order.ShippingAddress = dto.ShippingAddress;
        if (dto.ReceivingAddress != null)
            order.ReceivingAddress = dto.ReceivingAddress;
        if (dto.ShippingFees.HasValue)
            order.ShippingFees = dto.ShippingFees.Value;

        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        return ToDto(order);
    }

    public async Task<OrderDto> ShipOrderAsync(long orderId, int userId, ShipOrderDto dto)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        if (order.Product == null || order.Product.UserId != userId)
            throw new UnauthorizedAccessException("只有卖家可以确认发货");

        if (order.Status != "confirmed")
            throw new InvalidOperationException("只有已确认状态的订单可以发货");

        var oldStatus = order.Status;
        order.Status = "shipping";
        order.ShippingTime = DateTime.Now;
        order.TrackingNumber = dto.TrackingNumber;
        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "shipping",
            ChangeTime = DateTime.Now,
            OperatorId = userId,
            Note = dto.TrackingNumber != null ? $"卖家发货，物流单号：{dto.TrackingNumber}" : "卖家发货"
        });
        await _timelineRepo.SaveAsync();

        // 通知买家已发货
        await _notifications.NotifyAsync(
            order.BuyerId,
            "订单已发货",
            $"您的订单《{order.Product?.Name}》已发货" + (dto.TrackingNumber != null ? $"，物流单号：{dto.TrackingNumber}" : ""),
            "order",
            orderId);

        return ToDto(order);
    }

    public async Task<OrderDto> ReceiveOrderAsync(long orderId, int userId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        if (order.BuyerId != userId)
            throw new UnauthorizedAccessException("只有买家可以确认收货");

        if (order.Status != "shipping")
            throw new InvalidOperationException("只有运输中状态的订单可以确认收货");

        var oldStatus = order.Status;
        order.Status = "success";
        order.DeliveryTime = DateTime.Now;
        order.CompleteTime = DateTime.Now;

        // 商品标记为已售出
        if (order.Product != null)
        {
            order.Product.Status = ProductStatus.Sold;
            _productRepo.Update(order.Product);
        }

        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        if (order.Product != null)
            await _reputationService.ChangeCreditAsync(order.Product.UserId, CreditRules.OrderCompleted);

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "success",
            ChangeTime = DateTime.Now,
            OperatorId = userId,
            Note = "买家确认收货，订单完成"
        });
        await _timelineRepo.SaveAsync();

        // 通知卖家买家已收货
        if (order.Product != null)
            await _notifications.NotifyAsync(
                order.Product.UserId,
                "买家已确认收货",
                $"您的商品《{order.Product.Name}》买家已确认收货，订单完成",
                "order",
                orderId);

        return ToDto(order);
    }

    public async Task<OrderDto> CompleteOrderAsync(long orderId, int userId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        // 买家或卖家都可以确认完成（兜底接口）
        if (order.BuyerId != userId && (order.Product == null || order.Product.UserId != userId))
            throw new UnauthorizedAccessException("无权操作该订单");

        if (order.Status != "shipping")
            throw new InvalidOperationException("只有运输中的订单可以完成");

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

        if (order.Product != null)
            await _reputationService.ChangeCreditAsync(order.Product.UserId, CreditRules.OrderCompleted);

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "success",
            ChangeTime = DateTime.Now,
            OperatorId = userId,
            Note = "订单已完成"
        });
        await _timelineRepo.SaveAsync();

        return ToDto(order);
    }

    public async Task<List<OrderTimelineDto>> GetTimelineAsync(long orderId)
    {
        var timelines = await _timelineRepo.GetByPurchaseIdAsync(orderId);
        return timelines.Select(t => new OrderTimelineDto
        {
            TimelineId = t.TimelineId,
            OldStatus = t.OldStatus,
            NewStatus = t.NewStatus,
            ChangeTime = t.ChangeTime,
            OperatorId = t.OperatorId,
            Note = t.Note
        }).ToList();
    }

    public async Task CancelExpiredOrdersAsync(TimeSpan expiration)
    {
        var cutoff = DateTime.Now - expiration;
        var pendingOrders = await _purchaseRepo.GetByStatusAsync("pending");
        var expired = pendingOrders.Where(o => o.CreateTime < cutoff).ToList();

        foreach (var order in expired)
        {
            // 双重检查，避免并发场景下误取消
            if (order.Status != "pending")
                continue;

            var oldStatus = order.Status;
            order.Status = "cancel";
            order.CancelTime = DateTime.Now;

            // 恢复商品为在售
            if (order.Product != null)
            {
                order.Product.Status = ProductStatus.Available;
                _productRepo.Update(order.Product);
            }

            // 取消该订单的待支付记录，防止超时后支付回调误标记成功
            var pendingPayment = await _paymentRepo.GetPendingByPurchaseIdAsync(order.PurchaseId);
            if (pendingPayment != null)
            {
                pendingPayment.Status = PaymentStatus.Cancelled;
                pendingPayment.CancelTime = DateTime.Now;
                _paymentRepo.Update(pendingPayment);
            }

            _purchaseRepo.Update(order);

            await _timelineRepo.AddAsync(new OrderTimeline
            {
                PurchaseId = order.PurchaseId,
                OldStatus = oldStatus,
                NewStatus = "cancel",
                ChangeTime = DateTime.Now,
                OperatorId = order.BuyerId,
                Note = "系统自动取消：超时未付款"
            });
        }

        await _purchaseRepo.SaveAsync();
        await _timelineRepo.SaveAsync();
    }

    // ==================== DTO 映射 ====================

    private OrderDto ToDto(Purchase p) => new()
    {
        PurchaseId = p.PurchaseId,
        Status = p.Status,
        CreateTime = p.CreateTime,
        ExpireTime = p.Status == "pending" ? p.CreateTime.AddMinutes(_expireMinutes) : null,
        CancelTime = p.CancelTime,
        PayTime = p.PayTime,
        ShippingTime = p.ShippingTime,
        DeliveryTime = p.DeliveryTime,
        CompleteTime = p.CompleteTime,
        ShippingFees = p.ShippingFees,
        ResponsibleForShip = p.ResponsibleForShip,
        ShippingMethod = p.ShippingMethod,
        ShippingAddress = p.ShippingAddress,
        ReceivingAddress = p.ReceivingAddress,
        TrackingNumber = p.TrackingNumber,
        BuyerId = p.BuyerId,
        BuyerName = p.Buyer?.UserName,
        ProductId = p.ProductId,
        ProductName = p.Product?.Name,
        ProductPrice = p.Product?.Price ?? 0,
        ProductCoverImageId = p.Product?.Images?
            .OrderBy(i => i.ImgIndex)
            .FirstOrDefault()?.ImgFileId,
        AddressId = p.AddressId,
        AddressDetail = p.Address != null
            ? $"{p.Address.Name} {p.Address.DetailAddress} {p.Address.PhoneNumber}"
            : null,
        IsPickup = p.IsPickup == 1,
        ReviewId = p.Review?.ReviewId,
        Rating = p.Review?.Rating
    };

    private OrderListItemDto ToListItem(Purchase p)
    {
        var sellerId = p.Product?.UserId ?? 0;
        var sellerName = p.Product?.Seller?.UserName ?? "";
        return new OrderListItemDto
        {
            PurchaseId = p.PurchaseId,
            Status = p.Status,
            CreateTime = p.CreateTime,
            ExpireTime = p.Status == "pending" ? p.CreateTime.AddMinutes(_expireMinutes) : null,
            PayTime = p.PayTime,
            CompleteTime = p.CompleteTime,
            ShippingFees = p.ShippingFees,
            ProductId = p.ProductId,
            ProductName = p.Product?.Name,
            ProductPrice = p.Product?.Price ?? 0,
            ProductCoverImageId = p.Product?.Images?
                .OrderBy(i => i.ImgIndex)
                .FirstOrDefault()?.ImgFileId,
            BuyerId = p.BuyerId,
            BuyerName = p.Buyer?.UserName,
            SellerId = sellerId,
            SellerName = sellerName,
            HasReview = p.Review != null
        };
    }
}
