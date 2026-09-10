using Backend.Dtos.Communication;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

/// <summary>
/// 通知模块 — 展示系统公告和当前用户收到的警告
/// 说明：通知的"已读"目前为只读假象（公告表和警告表均无已读字段），
///       Read/ReadAll 仅校验存在性，实际未持久化已读状态
/// </summary>
[ApiController, Authorize, Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IAnnouncementRepository _announcements;
    private readonly IUserWarningRepository _warnings;

    public NotificationController(
        IAnnouncementRepository announcements,
        IUserWarningRepository warnings)
    {
        _announcements = announcements;
        _warnings = warnings;
    }

    private static NotificationDto MapAnnouncement(Models.Announcement a) => new()
    {
        NotificationId = a.AnnouncementId,
        Type = "announcement",
        Title = a.Title,
        Content = a.Content,
        CreateTime = a.PublishTime ?? a.ReleaseTime,
        IsRead = false
    };

    private static NotificationDto MapWarning(Models.UserWarning w) => new()
    {
        NotificationId = w.WarningId,
        Type = "warning",
        Title = "平台警告",
        Content = w.Reason,
        CreateTime = w.CreateTime,
        IsRead = false
    };

    private int CurrentUserId()
        => int.Parse(User.FindFirst("userId")!.Value);

    private async Task<Models.UserWarning?> FindWarningAsync(long warningId)
    {
        var userId = CurrentUserId();
        return await _warnings.Query()
            .FirstOrDefaultAsync(w =>
                w.UserId == userId && w.WarningId == warningId);
    }

    /// <summary>
    /// 通知列表（按创建/发布时间倒序，包含公告与当前用户的警告）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> List()
    {
        var userId = CurrentUserId();
        var warnings = await _warnings.Query()
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreateTime)
            .ToListAsync();
        var announcements = await _announcements.GetPublishedAsync();

        var notifications = warnings.Select(MapWarning)
            .Concat(announcements.Select(MapAnnouncement))
            .OrderByDescending(n => n.CreateTime)
            .ToList();

        return Ok(notifications);
    }

    /// <summary>
    /// 通知详情
    /// </summary>
    [HttpGet("{notificationId:long}")]
    public async Task<ActionResult<NotificationDto>> Get(
        long notificationId,
        [FromQuery] string? type = null)
    {
        if (string.Equals(type, "warning", StringComparison.OrdinalIgnoreCase))
        {
            var warning = await FindWarningAsync(notificationId);
            return warning == null ? NotFound() : Ok(MapWarning(warning));
        }

        var a = (await _announcements.GetPublishedAsync())
            .FirstOrDefault(x => x.AnnouncementId == notificationId);
        if (a != null) return Ok(MapAnnouncement(a));

        var fallbackWarning = await FindWarningAsync(notificationId);
        return fallbackWarning == null
            ? NotFound()
            : Ok(MapWarning(fallbackWarning));
    }

    /// <summary>
    /// 标记单条通知为已读（仅校验存在性，未持久化）
    /// </summary>
    [HttpPatch("{notificationId:long}/read")]
    public async Task<IActionResult> Read(
        long notificationId,
        [FromQuery] string? type = null)
    {
        if (string.Equals(type, "warning", StringComparison.OrdinalIgnoreCase))
            return await FindWarningAsync(notificationId) == null
                ? NotFound()
                : NoContent();

        var exists = (await _announcements.GetPublishedAsync())
            .Any(x => x.AnnouncementId == notificationId);
        if (!exists && await FindWarningAsync(notificationId) == null)
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
    [HttpDelete("{notificationId:long}")]
    public async Task<IActionResult> Delete(
        long notificationId,
        [FromQuery] string? type = null)
    {
        if (string.Equals(type, "warning", StringComparison.OrdinalIgnoreCase))
        {
            var warning = await FindWarningAsync(notificationId);
            if (warning == null) return NotFound();
            return BadRequest(new { error = "警告记录不支持删除" });
        }

        var a = (await _announcements.GetPublishedAsync())
            .FirstOrDefault(x => x.AnnouncementId == notificationId);
        if (a == null)
        {
            if (await FindWarningAsync(notificationId) != null)
                return BadRequest(new { error = "警告记录不支持删除" });
            return NotFound();
        }

        _announcements.Delete(a);
        await _announcements.SaveAsync();
        return NoContent();
    }
}
