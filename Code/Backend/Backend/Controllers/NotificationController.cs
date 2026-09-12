using Backend.Dtos.Communication;
using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

/// <summary>
/// 通知模块 — 展示系统公告、当前用户收到的警告，以及商品/订单动态等系统通知
/// 说明：公告与警告本身无已读字段（仍是只读假象），系统通知的已读状态会持久化
/// </summary>
[ApiController, Authorize, Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IAnnouncementRepository _announcements;
    private readonly IUserWarningRepository _warnings;
    private readonly INotificationService _notifications;

    public NotificationController(
        IAnnouncementRepository announcements,
        IUserWarningRepository warnings,
        INotificationService notifications)
    {
        _announcements = announcements;
        _warnings = warnings;
        _notifications = notifications;
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
        var system = await _notifications.GetByUserIdAsync(userId);

        var notifications = warnings.Select(MapWarning)
            .Concat(announcements.Select(MapAnnouncement))
            .Concat(system)
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

        if (string.Equals(type, "announcement", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(type))
        {
            var exists = (await _announcements.GetPublishedAsync())
                .Any(x => x.AnnouncementId == notificationId);
            if (!exists && await FindWarningAsync(notificationId) == null)
                return NotFound();
            return NoContent();
        }

        // 系统通知：真正持久化已读状态
        var ok = await _notifications.MarkReadAsync(notificationId, CurrentUserId());
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// 全部标记为已读（目前持久化系统通知）
    /// </summary>
    [HttpPatch("read-all")]
    public async Task<IActionResult> ReadAll()
    {
        await _notifications.MarkAllReadAsync(CurrentUserId());
        return NoContent();
    }

    /// <summary>
    /// 删除通知
    /// </summary>
    [HttpDelete("{notificationId:long}")]
    public async Task<IActionResult> Delete(
        long notificationId,
        [FromQuery] string? type = null)
    {
        var userId = CurrentUserId();

        if (string.Equals(type, "warning", StringComparison.OrdinalIgnoreCase))
        {
            var warning = await FindWarningAsync(notificationId);
            if (warning == null) return NotFound();
            return BadRequest(new { error = "警告记录不支持删除" });
        }

        if (string.Equals(type, "announcement", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(type))
        {
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

        // 系统通知
        if (await _notifications.DeleteAsync(notificationId, userId))
            return NoContent();
        return NotFound();
    }
}
