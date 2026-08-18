using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员用户信息查询模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public class AdminUserController : ControllerBase
{
    private readonly IAdminUserManagementService _service;

    public AdminUserController(IAdminUserManagementService service)
    {
        _service = service;
    }

    /// <summary>
    /// 管理员查询用户列表，支持关键词、状态、注册时间、信誉筛选
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminUserPageDto>> GetUsers(
        [FromQuery] string? keyword = null,
        [FromQuery] int? userType = null,
        [FromQuery] int? accountStatus = null,
        [FromQuery] int? creditMin = null,
        [FromQuery] int? creditMax = null,
        [FromQuery] DateTime? registerStart = null,
        [FromQuery] DateTime? registerEnd = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetUsersAsync(
            keyword, userType, accountStatus, creditMin, creditMax, registerStart, registerEnd, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 获取用户数量、活跃度、违规数量等统计数据
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AdminUserStatisticsDto>> GetStatistics()
    {
        return Ok(await _service.GetStatisticsAsync());
    }

    /// <summary>
    /// 管理员查看用户详情
    /// </summary>
    [HttpGet("{userId:int}")]
    public async Task<ActionResult<AdminUserDetailDto>> GetUser(int userId)
    {
        var user = await _service.GetUserDetailAsync(userId);
        if (user == null) return NotFound(new { error = "用户不存在" });
        return Ok(user);
    }

    /// <summary>
    /// 查看某用户发布的商品
    /// </summary>
    [HttpGet("{userId:int}/products")]
    public async Task<ActionResult> GetUserProducts(int userId)
    {
        var products = await _service.GetUserProductsAsync(userId);
        if (products == null) return NotFound(new { error = "用户不存在" });
        return Ok(products);
    }

    /// <summary>
    /// 查看某用户相关订单（买入和卖出）
    /// </summary>
    [HttpGet("{userId:int}/orders")]
    public async Task<ActionResult> GetUserOrders(int userId)
    {
        var orders = await _service.GetUserOrdersAsync(userId);
        if (orders == null) return NotFound(new { error = "用户不存在" });
        return Ok(orders);
    }

    /// <summary>
    /// 查看某用户相关举报记录
    /// </summary>
    [HttpGet("{userId:int}/reports")]
    public async Task<ActionResult> GetUserReports(int userId)
    {
        var reports = await _service.GetUserReportsAsync(userId);
        if (reports == null) return NotFound(new { error = "用户不存在" });
        return Ok(reports);
    }

    /// <summary>
    /// 查看某用户申诉记录
    /// </summary>
    [HttpGet("{userId:int}/appeals")]
    public async Task<ActionResult> GetUserAppeals(int userId)
    {
        var appeals = await _service.GetUserAppealsAsync(userId);
        if (appeals == null) return NotFound(new { error = "用户不存在" });
        return Ok(appeals);
    }

    /// <summary>
    /// 查看某用户信誉和违规概览
    /// </summary>
    [HttpGet("{userId:int}/reputation")]
    public async Task<ActionResult<AdminUserReputationDto>> GetUserReputation(int userId)
    {
        try
        {
            return Ok(await _service.GetUserReputationAsync(userId));
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 修改用户状态：正常、禁言、限制发布、封禁
    /// </summary>
    [HttpPatch("{userId:int}/status")]
    public async Task<ActionResult<AdminUserDetailDto>> UpdateUserStatus(
        int userId,
        [FromBody] UpdateAdminUserStatusDto dto)
    {
        try
        {
            var user = await _service.UpdateUserStatusAsync(userId, dto);
            if (user == null) return NotFound(new { error = "用户不存在" });
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 向用户发送警告通知
    /// </summary>
    [HttpPost("{userId:int}/warning")]
    public async Task<ActionResult<AdminUserWarningDto>> CreateWarning(
        int userId,
        [FromBody] CreateUserWarningDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);

        try
        {
            var warning = await _service.CreateWarningAsync(userId, dto, adminId);
            if (warning == null) return NotFound(new { error = "用户不存在" });
            return Ok(warning);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
