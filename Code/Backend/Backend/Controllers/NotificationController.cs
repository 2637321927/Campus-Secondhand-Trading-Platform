using Backend.Dtos.Communication;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 通知模块 — 基于公告表 announcement 展示给用户的通知
/// 说明：通知的"已读"目前为只读假象（Map 固定 IsRead=false，公告表无已读字段），
///       Read/ReadAll 仅校验存在性，实际未持久化已读状态
/// </summary>
[ApiController, Authorize, Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IAnnouncementRepository _announcements;

    public NotificationController(IAnnouncementRepository announcements)
        => _announcements = announcements;

    private static NotificationDto Map(Models.Announcement a) => new()
    {
        NotificationId = a.AnnouncementId,
        Title = a.Title,
        Content = a.Info,
        CreateTime = a.ReleaseTime,
        IsRead = false
    };

    /// <summary>
    /// 通知列表（按发布时间倒序）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> List()
        => Ok((await _announcements.GetAllAsync())
            .OrderByDescending(x => x.ReleaseTime)
            .Select(Map));

    /// <summary>
    /// 通知详情
    /// </summary>
    [HttpGet("{notificationId:int}")]
    public async Task<ActionResult<NotificationDto>> Get(int notificationId)
    {
        var a = await _announcements.GetByIdAsync(notificationId);
        return a == null ? NotFound() : Ok(Map(a));
    }

    /// <summary>
    /// 标记单条通知为已读（仅校验存在性，未持久化）
    /// </summary>
    [HttpPatch("{notificationId:int}/read")]
    public async Task<IActionResult> Read(int notificationId)
    {
        if (await _announcements.GetByIdAsync(notificationId) == null)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// 全部标记为已读（空实现）
    /// </summary>
    [HttpPatch("read-all")]
    public IActionResult ReadAll() => NoContent();

    /// <summary>
    /// 删除通知
    /// </summary>
    [HttpDelete("{notificationId:int}")]
    public async Task<IActionResult> Delete(int notificationId)
    {
        var a = await _announcements.GetByIdAsync(notificationId);
        if (a == null) return NotFound();

        _announcements.Delete(a);
        await _announcements.SaveAsync();
        return NoContent();
    }
}
