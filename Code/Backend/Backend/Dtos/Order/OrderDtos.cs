using Backend.Models.Enums;

namespace Backend.Dtos.Order;

/// <summary>
/// 创建订单请求
/// </summary>
public class CreateOrderDto
{
    /// <summary>
    /// 购买的商品ID
    /// </summary>
    public long ProductId { get; set; }

    /// <summary>
    /// 收货地址ID（从用户地址簿选择）
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// 发货方式，例如"快递"、"自提"、"面交"
    /// </summary>
    public string? ShippingMethod { get; set; }

    /// <summary>
    /// 买家备注
    /// </summary>
    public string? Note { get; set; }
}

/// <summary>
/// 订单详情响应
/// </summary>
public class OrderDto
{
    public long PurchaseId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreateTime { get; set; }
    public DateTime? CancelTime { get; set; }
    public DateTime? PayTime { get; set; }
    public DateTime? ShippingTime { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public decimal ShippingFees { get; set; }
    public int ResponsibleForShip { get; set; }
    public string? ShippingMethod { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ReceivingAddress { get; set; }
    public string? TrackingNumber { get; set; }

    // 关联信息
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public long? ProductCoverImageId { get; set; }
    public int AddressId { get; set; }
    public string? AddressDetail { get; set; }

    // 评价信息
    public int? ReviewId { get; set; }
    public int? Rating { get; set; }
}

/// <summary>
/// 订单列表项（简化响应，用于列表展示）
/// </summary>
public class OrderListItemDto
{
    public long PurchaseId { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreateTime { get; set; }
    public DateTime? PayTime { get; set; }
    public DateTime? CompleteTime { get; set; }
    public decimal ShippingFees { get; set; }

    // 商品信息
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public long? ProductCoverImageId { get; set; }

    // 对方用户信息
    public int BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public int SellerId { get; set; }
    public string? SellerName { get; set; }

    // 是否已评价
    public bool HasReview { get; set; }
}

/// <summary>
/// 购买可行性检查响应
/// </summary>
public class PurchaseCheckDto
{
    /// <summary>
    /// 是否可购买
    /// </summary>
    public bool CanPurchase { get; set; }

    /// <summary>
    /// 不可购买的原因（如果有）
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 商品状态
    /// </summary>
    public string ProductStatus { get; set; } = string.Empty;

    /// <summary>
    /// 是否为本人商品
    /// </summary>
    public bool IsOwnProduct { get; set; }

    /// <summary>
    /// 发货方式列表（继承自商品）
    /// </summary>
    public ShippingType ShippingType { get; set; }
}

/// <summary>
/// 修改发货信息请求
/// </summary>
public class UpdateShippingDto
{
    /// <summary>
    /// 发货方式
    /// </summary>
    public string? ShippingMethod { get; set; }

    /// <summary>
    /// 发货地址
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// 收货地址
    /// </summary>
    public string? ReceivingAddress { get; set; }

    /// <summary>
    /// 运费金额
    /// </summary>
    public decimal? ShippingFees { get; set; }
}

/// <summary>
/// 确认发货请求
/// </summary>
public class ShipOrderDto
{
    /// <summary>
    /// 快递单号/物流单号
    /// </summary>
    public string? TrackingNumber { get; set; }
}

/// <summary>
/// 订单状态流转记录响应
/// </summary>
public class OrderTimelineDto
{
    public long TimelineId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public DateTime ChangeTime { get; set; }
    public int OperatorId { get; set; }
    public string? Note { get; set; }
}
