using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController, Authorize, Route("api/messages")]
public class MessageController : ControllerBase
{
    private readonly Data.AppDbContext _db;
    public MessageController(Data.AppDbContext db) => _db = db;
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> UnreadCount()
    {
        var uid=int.Parse(User.FindFirst("userId")!.Value);
        var n=await _db.Messages.CountAsync(m=>m.SenderId!=uid && m.IsRead==0 && _db.Conversations.Any(c=>c.SessionId==m.SessionId && (c.BuyerId==uid || _db.Products.Any(p=>p.ProductId==c.ProductId&&p.UserId==uid))));
        return Ok(n);
    }
}
