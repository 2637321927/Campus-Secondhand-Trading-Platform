using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员订单管理模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/orders")]
public class AdminOrderController : ControllerBase
{
    private readonly IAdminOrderService _service;

    public AdminOrderController(IAdminOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<AdminOrderPageDto>> GetOrders(
        [FromQuery] long? orderId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetOrdersAsync(orderId, status, startDate, endDate, page, pageSize));
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<AdminOrderStatisticsDto>> GetStatistics()
    {
        return Ok(await _service.GetStatisticsAsync());
    }

    [HttpGet("{orderId:long}")]
    public async Task<ActionResult<AdminOrderDto>> GetOrder(long orderId)
    {
        var order = await _service.GetOrderAsync(orderId);
        if (order == null) return NotFound(new { error = "订单不存在" });
        return Ok(order);
    }

    [HttpPatch("{orderId:long}/cancel")]
    public async Task<ActionResult<AdminOrderDto>> Cancel(long orderId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.CancelOrderAsync(orderId, adminId));
    }

    [HttpPatch("{orderId:long}/complete")]
    public async Task<ActionResult<AdminOrderDto>> Complete(long orderId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.CompleteOrderAsync(orderId, adminId));
    }

    private async Task<ActionResult<AdminOrderDto>> RunActionAsync(
        Func<Task<AdminOrderDto?>> action)
    {
        try
        {
            var order = await action();
            if (order == null) return NotFound(new { error = "订单不存在" });
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
