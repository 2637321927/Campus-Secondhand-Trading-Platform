using Backend.Dtos.Communication;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController, Authorize, Route("api/notifications")]
public class NotificationController : ControllerBase
{
    private readonly IAnnouncementRepository _announcements;
    public NotificationController(IAnnouncementRepository announcements) => _announcements = announcements;
    private static NotificationDto Map(Models.Announcement a) => new() { NotificationId=a.AnnouncementId, Title=a.Title, Content=a.Info, CreateTime=a.ReleaseTime, IsRead=false };
    [HttpGet] public async Task<ActionResult<List<NotificationDto>>> List() => Ok((await _announcements.GetAllAsync()).OrderByDescending(x=>x.ReleaseTime).Select(Map));
    [HttpGet("{notificationId:int}")] public async Task<ActionResult<NotificationDto>> Get(int notificationId){var a=await _announcements.GetByIdAsync(notificationId);return a==null?NotFound():Ok(Map(a));}
    [HttpPatch("{notificationId:int}/read")] public async Task<IActionResult> Read(int notificationId){if(await _announcements.GetByIdAsync(notificationId)==null)return NotFound();return NoContent();}
    [HttpPatch("read-all")] public IActionResult ReadAll()=>NoContent();
    [HttpDelete("{notificationId:int}")] public async Task<IActionResult> Delete(int notificationId){var a=await _announcements.GetByIdAsync(notificationId);if(a==null)return NotFound();_announcements.Delete(a);await _announcements.SaveAsync();return NoContent();}
}
