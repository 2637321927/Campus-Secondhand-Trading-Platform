using Backend.Dtos.Communication;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员公告管理模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/announcements")]
public class AdminAnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _service;

    public AdminAnnouncementController(IAnnouncementService service)
    {
        _service = service;
    }

    /// <summary>
    /// 管理员发布公告
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AnnouncementDto>> Create(
        [FromBody] CreateAnnouncementDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        var announcement = await _service.CreateAsync(adminId, dto);
        return Ok(announcement);
    }

    /// <summary>
    /// 管理员查看全部公告
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AnnouncementDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }
}
