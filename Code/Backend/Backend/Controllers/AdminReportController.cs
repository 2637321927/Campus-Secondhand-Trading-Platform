using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员举报处理模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/reports")]
public class AdminReportController : ControllerBase
{
    private readonly IAdminModerationService _service;

    public AdminReportController(IAdminModerationService service)
    {
        _service = service;
    }

    /// <summary>
    /// 管理员查询举报列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminModerationPageDto>> GetReports(
        [FromQuery] string? keyword = null,
        [FromQuery] string? status = null,
        [FromQuery] string? targetType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetReportsAsync(keyword, status, targetType, page, pageSize));
    }

    /// <summary>
    /// 管理员查看举报详情
    /// </summary>
    [HttpGet("{reportId:long}")]
    public async Task<ActionResult<AdminModerationDetailDto>> GetReport(long reportId)
    {
        var report = await _service.GetReportDetailAsync(reportId);
        if (report == null) return NotFound(new { error = "举报不存在" });
        return Ok(report);
    }

    /// <summary>
    /// 判定举报成立
    /// </summary>
    [HttpPatch("{reportId:long}/accept")]
    public async Task<ActionResult<AdminModerationDetailDto>> Accept(long reportId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.AcceptReportAsync(reportId, adminId));
    }

    /// <summary>
    /// 判定举报不成立
    /// </summary>
    [HttpPatch("{reportId:long}/reject")]
    public async Task<ActionResult<AdminModerationDetailDto>> Reject(long reportId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.RejectReportAsync(reportId, adminId));
    }

    /// <summary>
    /// 对举报进行综合处理
    /// </summary>
    [HttpPatch("{reportId:long}/handle")]
    public async Task<ActionResult<AdminModerationDetailDto>> Handle(
        long reportId,
        [FromBody] HandleWorkOrderDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.HandleReportAsync(reportId, dto, adminId));
    }

    private async Task<ActionResult<AdminModerationDetailDto>> RunActionAsync(
        Func<Task<AdminModerationDetailDto?>> action)
    {
        try
        {
            var result = await action();
            if (result == null) return NotFound(new { error = "举报不存在" });
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
