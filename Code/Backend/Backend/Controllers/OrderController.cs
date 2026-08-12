using Backend.Dtos.Order;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 购买、订单模块
/// </summary>
[ApiController]
[Route("api")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// 检查商品是否可购买
    /// </summary>
    [Authorize]
    [HttpGet("products/{productId}/purchase-check")]
    public async Task<ActionResult<PurchaseCheckDto>> PurchaseCheck(long productId)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var result = await _orderService.PurchaseCheckAsync(productId, userId);
        return Ok(result);
    }

    /// <summary>
    /// 创建购买订单
    /// </summary>
    [Authorize]
    [HttpPost("orders")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.CreateOrderAsync(userId, dto);
            return CreatedAtAction(nameof(GetOrder), new { orderId = order.PurchaseId }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取订单详情
    /// </summary>
    [HttpGet("orders/{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrder(long orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null) return NotFound(new { error = "订单不存在" });
        return Ok(order);
    }

    /// <summary>
    /// 获取当前用户购买订单列表
    /// </summary>
    [Authorize]
    [HttpGet("orders/me/buying")]
    public async Task<ActionResult<List<OrderListItemDto>>> GetBuyingOrders()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var orders = await _orderService.GetBuyingOrdersAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// 获取当前用户卖出订单列表
    /// </summary>
    [Authorize]
    [HttpGet("orders/me/selling")]
    public async Task<ActionResult<List<OrderListItemDto>>> GetSellingOrders()
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var orders = await _orderService.GetSellingOrdersAsync(userId);
        return Ok(orders);
    }

    /// <summary>
    /// 取消订单（买家在待付款状态下取消）
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/cancel")]
    public async Task<ActionResult<OrderDto>> CancelOrder(long orderId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.CancelOrderAsync(orderId, userId);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 卖家确认订单
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/seller-confirm")]
    public async Task<ActionResult<OrderDto>> SellerConfirm(long orderId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.SellerConfirmAsync(orderId, userId);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 卖家拒绝订单
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/seller-reject")]
    public async Task<ActionResult<OrderDto>> SellerReject(long orderId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.SellerRejectAsync(orderId, userId);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 设置或修改订单发货信息
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/shipping")]
    public async Task<ActionResult<OrderDto>> UpdateShipping(long orderId, [FromBody] UpdateShippingDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.UpdateShippingAsync(orderId, userId, dto);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 卖家确认发货
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/ship")]
    public async Task<ActionResult<OrderDto>> ShipOrder(long orderId, [FromBody] ShipOrderDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.ShipOrderAsync(orderId, userId, dto);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 买家确认收货
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/receive")]
    public async Task<ActionResult<OrderDto>> ReceiveOrder(long orderId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.ReceiveOrderAsync(orderId, userId);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 完成订单
    /// </summary>
    [Authorize]
    [HttpPatch("orders/{orderId}/complete")]
    public async Task<ActionResult<OrderDto>> CompleteOrder(long orderId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var order = await _orderService.CompleteOrderAsync(orderId, userId);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 获取订单状态流转记录
    /// </summary>
    [HttpGet("orders/{orderId}/timeline")]
    public async Task<ActionResult<List<OrderTimelineDto>>> GetTimeline(long orderId)
    {
        var timeline = await _orderService.GetTimelineAsync(orderId);
        return Ok(timeline);
    }
}
