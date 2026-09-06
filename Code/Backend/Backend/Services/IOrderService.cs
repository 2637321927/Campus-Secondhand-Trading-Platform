using Backend.Dtos.Order;

namespace Backend.Services;

public interface IOrderService
{
    /// <summary>
    /// 检查商品是否可购买
    /// </summary>
    Task<PurchaseCheckDto> PurchaseCheckAsync(long productId, int userId);

    /// <summary>
    /// 创建订单
    /// </summary>
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderDto dto);

    /// <summary>
    /// 获取订单详情
    /// </summary>
    Task<OrderDto?> GetOrderByIdAsync(long orderId);

    /// <summary>
    /// 获取当前用户的购买订单列表
    /// </summary>
    Task<List<OrderListItemDto>> GetBuyingOrdersAsync(int userId);

    /// <summary>
    /// 获取当前用户的卖出订单列表
    /// </summary>
    Task<List<OrderListItemDto>> GetSellingOrdersAsync(int userId);

    /// <summary>
    /// 取消订单（买家在待付款状态下取消）
    /// </summary>
    Task<OrderDto> CancelOrderAsync(long orderId, int userId);

    /// <summary>
    /// 卖家确认订单
    /// </summary>
    Task<OrderDto> SellerConfirmAsync(long orderId, int userId);

    /// <summary>
    /// 卖家拒绝订单
    /// </summary>
    Task<OrderDto> SellerRejectAsync(long orderId, int userId);

    /// <summary>
    /// 设置或修改发货信息
    /// </summary>
    Task<OrderDto> UpdateShippingAsync(long orderId, int userId, UpdateShippingDto dto);

    /// <summary>
    /// 卖家确认发货
    /// </summary>
    Task<OrderDto> ShipOrderAsync(long orderId, int userId, ShipOrderDto dto);

    /// <summary>
    /// 买家确认收货
    /// </summary>
    Task<OrderDto> ReceiveOrderAsync(long orderId, int userId);

    /// <summary>
    /// 完成订单
    /// </summary>
    Task<OrderDto> CompleteOrderAsync(long orderId, int userId);

    /// <summary>
    /// 获取订单状态流转记录
    /// </summary>
    Task<List<OrderTimelineDto>> GetTimelineAsync(long orderId);
}
