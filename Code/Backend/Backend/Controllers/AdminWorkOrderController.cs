using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员工单处理模块（统一举报与申诉）
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/work-orders")]
public class AdminWorkOrderController : ControllerBase
{
    private readonly IAdminModerationService _service;

    public AdminWorkOrderController(IAdminModerationService service)
    {
        _service = service;
    }

    /// <summary>
    /// 管理员查询工单列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminModerationPageDto>> GetWorkOrders(
        [FromQuery] string? type = null,
        [FromQuery] string? keyword = null,
        [FromQuery] string? status = null,
        [FromQuery] string? targetType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetWorkOrdersAsync(type, keyword, status, targetType, page, pageSize));
    }

    /// <summary>
    /// 管理员查看工单详情
    /// </summary>
    [HttpGet("{workOrderId:long}")]
    public async Task<ActionResult<AdminModerationDetailDto>> GetWorkOrder(long workOrderId)
    {
        var workOrder = await _service.GetWorkOrderDetailAsync(workOrderId);
        if (workOrder == null) return NotFound(new { error = "工单不存在" });
        return Ok(workOrder);
    }

    /// <summary>
    /// 驳回工单
    /// </summary>
    [HttpPatch("{workOrderId:long}/reject")]
    public async Task<ActionResult<AdminModerationDetailDto>> Reject(long workOrderId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.RejectWorkOrderAsync(workOrderId, adminId));
    }

    /// <summary>
    /// 处理工单
    /// </summary>
    [HttpPatch("{workOrderId:long}/handle")]
    public async Task<ActionResult<AdminModerationDetailDto>> Handle(
        long workOrderId,
        [FromBody] HandleWorkOrderDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.ProcessWorkOrderAsync(workOrderId, dto, adminId));
    }

    private async Task<ActionResult<AdminModerationDetailDto>> RunActionAsync(
        Func<Task<AdminModerationDetailDto?>> action)
    {
        try
        {
            var result = await action();
            if (result == null) return NotFound(new { error = "工单不存在" });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
