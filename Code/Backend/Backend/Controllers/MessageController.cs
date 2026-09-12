using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

/// <summary>
/// 消息模块 — 站内未读消息统计
/// </summary>
[ApiController, Authorize, Route("api/messages")]
public class MessageController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public MessageController(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    /// <summary>
    /// 当前用户未读数 = 所有会话中的未读消息数 + 未读系统通知数
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount()
    {
        var uid = int.Parse(User.FindFirst("userId")!.Value);

        // 未读 = 别人发给我（SenderId != uid、IsRead == 0），
        // 且该消息所在会话我必须是参与方（买家本人或商品卖家）
        var messageCount = await _db.Messages.CountAsync(m =>
            m.SenderId != uid
            && m.IsRead == 0
            && _db.Conversations.Any(c =>
                c.SessionId == m.SessionId
                && (c.BuyerId == uid
                    || _db.Products.Any(p => p.ProductId == c.ProductId && p.UserId == uid))));

        // 未读系统通知（商品动态、订单动态等）
        var notificationCount = await _notifications.GetUnreadCountAsync(uid);

        return Ok(messageCount + notificationCount);
    }
}