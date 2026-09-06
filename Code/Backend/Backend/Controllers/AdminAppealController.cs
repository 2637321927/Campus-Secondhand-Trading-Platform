using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员申诉处理模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/appeals")]
public class AdminAppealController : ControllerBase
{
    private readonly IAdminModerationService _service;

    public AdminAppealController(IAdminModerationService service)
    {
        _service = service;
    }

    /// <summary>
    /// 管理员查询申诉列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminModerationPageDto>> GetAppeals(
        [FromQuery] string? keyword = null,
        [FromQuery] string? status = null,
        [FromQuery] string? targetType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetAppealsAsync(keyword, status, targetType, page, pageSize));
    }

    /// <summary>
    /// 管理员查看申诉详情
    /// </summary>
    [HttpGet("{appealId:long}")]
    public async Task<ActionResult<AdminModerationDetailDto>> GetAppeal(long appealId)
    {
        var appeal = await _service.GetAppealDetailAsync(appealId);
        if (appeal == null) return NotFound(new { error = "申诉不存在" });
        return Ok(appeal);
    }

    /// <summary>
    /// 申诉通过，恢复商品或解除限制
    /// </summary>
    [HttpPatch("{appealId:long}/approve")]
    public async Task<ActionResult<AdminModerationDetailDto>> Approve(long appealId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.ApproveAppealAsync(appealId, adminId));
    }

    /// <summary>
    /// 申诉驳回
    /// </summary>
    [HttpPatch("{appealId:long}/reject")]
    public async Task<ActionResult<AdminModerationDetailDto>> Reject(long appealId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.RejectAppealAsync(appealId, adminId));
    }

    /// <summary>
    /// 管理员回复申诉处理意见
    /// </summary>
    [HttpPost("{appealId:long}/reply")]
    public async Task<ActionResult<AdminModerationDetailDto>> Reply(
        long appealId,
        [FromBody] WorkOrderReplyDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.ReplyAppealAsync(appealId, dto, adminId));
    }

    private async Task<ActionResult<AdminModerationDetailDto>> RunActionAsync(
        Func<Task<AdminModerationDetailDto?>> action)
    {
        try
        {
            var result = await action();
            if (result == null) return NotFound(new { error = "申诉不存在" });
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
