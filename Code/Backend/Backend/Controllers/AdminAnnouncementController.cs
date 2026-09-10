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
    /// 管理员分页查看公告
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AnnouncementPageDto>> GetPage(
        [FromQuery] string? keyword = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetPageAsync(keyword, status, page, pageSize));
    }

    /// <summary>
    /// 公告统计
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AnnouncementStatisticsDto>> GetStatistics()
    {
        return Ok(await _service.GetStatisticsAsync());
    }

    /// <summary>
    /// 公告详情
    /// </summary>
    [HttpGet("{announcementId:int}")]
    public async Task<ActionResult<AnnouncementDto>> GetById(int announcementId)
    {
        var announcement = await _service.GetByIdAsync(announcementId);
        if (announcement == null) return NotFound(new { error = "公告不存在" });
        return Ok(announcement);
    }

    /// <summary>
    /// 修改公告
    /// </summary>
    [HttpPut("{announcementId:int}")]
    public async Task<ActionResult<AnnouncementDto>> Update(
        int announcementId,
        [FromBody] UpdateAnnouncementDto dto)
    {
        var announcement = await _service.UpdateAsync(announcementId, dto);
        if (announcement == null) return NotFound(new { error = "公告不存在" });
        return Ok(announcement);
    }

    /// <summary>
    /// 发布公告
    /// </summary>
    [HttpPatch("{announcementId:int}/publish")]
    public async Task<ActionResult<AnnouncementDto>> Publish(int announcementId)
    {
        var announcement = await _service.SetStatusAsync(announcementId, "published");
        if (announcement == null) return NotFound(new { error = "公告不存在" });
        return Ok(announcement);
    }

    /// <summary>
    /// 下架公告
    /// </summary>
    [HttpPatch("{announcementId:int}/archive")]
    public async Task<ActionResult<AnnouncementDto>> Archive(int announcementId)
    {
        var announcement = await _service.SetStatusAsync(announcementId, "archived");
        if (announcement == null) return NotFound(new { error = "公告不存在" });
        return Ok(announcement);
    }

    /// <summary>
    /// 删除公告
    /// </summary>
    [HttpDelete("{announcementId:int}")]
    public async Task<IActionResult> Delete(int announcementId)
    {
        var deleted = await _service.DeleteAsync(announcementId);
        if (!deleted) return NotFound(new { error = "公告不存在" });
        return NoContent();
    }
}
