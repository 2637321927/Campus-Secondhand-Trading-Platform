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

    public OrderService(
        IPurchaseRepository purchaseRepo,
        IProductRepository productRepo,
        IAddressRepository addressRepo,
        IOrderTimelineRepository timelineRepo)
    {
        _purchaseRepo = purchaseRepo;
        _productRepo = productRepo;
        _addressRepo = addressRepo;
        _timelineRepo = timelineRepo;
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
            reason = "商品当前不可购买（已售出或已下架）";
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

        var address = await _addressRepo.GetByIdAsync(dto.AddressId);
        if (address == null || address.UserId != userId)
            throw new ArgumentException("收货地址无效");

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
            AddressId = dto.AddressId,
            ShippingMethod = dto.ShippingMethod,
            ShippingFees = product.ShippingFee ?? 0,
            ResponsibleForShip = product.ShippingType == ShippingType.Free ? 0 : 1
        };

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
            Note = "创建订单"
        });
        await _timelineRepo.SaveAsync();

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

        if (order.Status != "pending")
            throw new InvalidOperationException("只有待付款状态的订单可以确认");

        var oldStatus = order.Status;
        order.Status = "paid";
        order.PayTime = DateTime.Now;
        _purchaseRepo.Update(order);
        await _purchaseRepo.SaveAsync();

        await _timelineRepo.AddAsync(new OrderTimeline
        {
            PurchaseId = orderId,
            OldStatus = oldStatus,
            NewStatus = "paid",
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

        if (order.Status != "pending")
            throw new InvalidOperationException("只有待付款状态的订单可以拒绝");

        var oldStatus = order.Status;
        order.Status = "cancel";
        order.CancelTime = DateTime.Now;
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

        if (order.Status == "shipping" || order.Status == "success" || order.Status == "cancel")
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

        if (order.Status != "paid")
            throw new InvalidOperationException("只有已付款状态的订单可以发货");

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

        return ToDto(order);
    }

    public async Task<OrderDto> CompleteOrderAsync(long orderId, int userId)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        // 买家或卖家都可以确认完成（兜底接口）
        if (order.BuyerId != userId && (order.Product == null || order.Product.UserId != userId))
            throw new UnauthorizedAccessException("无权操作该订单");

        if (order.Status == "cancel" || order.Status == "success")
            throw new InvalidOperationException("订单已结束，无法重复完成");

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

    // ==================== DTO 映射 ====================

    private static OrderDto ToDto(Purchase p) => new()
    {
        PurchaseId = p.PurchaseId,
        Status = p.Status,
        CreateTime = p.CreateTime,
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
        ReviewId = p.Review?.ReviewId,
        Rating = p.Review?.Rating
    };

    private static OrderListItemDto ToListItem(Purchase p)
    {
        var sellerId = p.Product?.UserId ?? 0;
        var sellerName = p.Product?.Seller?.UserName ?? "";
        return new OrderListItemDto
        {
            PurchaseId = p.PurchaseId,
            Status = p.Status,
            CreateTime = p.CreateTime,
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
