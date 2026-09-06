using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员待处理任务模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/moderation")]
public class AdminModerationController : ControllerBase
{
    private readonly IAdminModerationService _service;

    public AdminModerationController(IAdminModerationService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取管理员待处理任务数量和列表
    /// </summary>
    [HttpGet("tasks")]
    public async Task<ActionResult<AdminModerationTasksDto>> GetTasks()
    {
        return Ok(await _service.GetTasksAsync());
    }
}
