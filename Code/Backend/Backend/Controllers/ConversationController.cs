using Backend.Data;
using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController, Authorize, Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IConversationRepository _conversations; private readonly IMessageRepository _messages; private readonly Backend.Services.IUpdatedFileService _files;
    public ConversationController(AppDbContext db, IConversationRepository conversations, IMessageRepository messages, Backend.Services.IUpdatedFileService files) { _db = db; _conversations = conversations; _messages = messages; _files = files; }
    private int UserId => int.Parse(User.FindFirst("userId")!.Value);
    private async Task<bool> CanAccess(Conversation c) => c.BuyerId == UserId || await _db.Products.AnyAsync(p => p.ProductId == c.ProductId && p.UserId == UserId);
    private static MessageDto MessageToDto(Message m) => new() { ConversationId=m.SessionId, MessageId=m.MsgIndex, SenderId=m.SenderId, MessageType=m.MsgType, FileId=m.FileId, Content=m.MsgContent, SendTime=m.SendTime, IsRead=m.IsRead == 1 };
    private async Task<ConversationDto> ToDto(Conversation c)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == c.ProductId);
        var unread = await _db.Messages.CountAsync(m => m.SessionId == c.SessionId && m.SenderId != UserId && m.IsRead == 0);
        return new ConversationDto { ConversationId=c.SessionId, ProductId=c.ProductId, ProductName=product?.Name ?? string.Empty, BuyerId=c.BuyerId, SellerId=product?.UserId ?? 0, CreateTime=c.CreateTime, UnreadCount=unread };
    }
    [HttpGet] public async Task<ActionResult<List<ConversationDto>>> List([FromQuery] string? keyword = null)
    { var ids = await _db.Conversations.Where(c => c.BuyerId == UserId || _db.Products.Any(p => p.ProductId == c.ProductId && p.UserId == UserId)).Select(c => c.SessionId).ToListAsync(); var list = new List<ConversationDto>(); foreach (var id in ids) { var c=await _db.Conversations.FindAsync(id); if(c!=null) { var d=await ToDto(c); if(string.IsNullOrWhiteSpace(keyword)||d.ProductName.Contains(keyword,StringComparison.OrdinalIgnoreCase)) list.Add(d); } } return Ok(list.OrderByDescending(x=>x.CreateTime)); }
    [HttpPost] public async Task<ActionResult<ConversationDto>> Create(CreateConversationDto dto)
    { var p=await _db.Products.FindAsync(dto.ProductId); if(p==null) return NotFound(); if(p.UserId==UserId) return BadRequest("不能与自己商品创建会话"); var c=await _db.Conversations.FirstOrDefaultAsync(x=>x.ProductId==dto.ProductId&&x.BuyerId==UserId); if(c==null){c=new Conversation{ProductId=dto.ProductId,BuyerId=UserId}; await _conversations.AddAsync(c); await _conversations.SaveAsync();} return Ok(await ToDto(c)); }
    [HttpGet("{conversationId:int}")] public async Task<ActionResult<ConversationDto>> Get(int conversationId) { var c=await _db.Conversations.FindAsync(conversationId); if(c==null||!await CanAccess(c)) return NotFound(); return Ok(await ToDto(c)); }
    [HttpDelete("{conversationId:int}")] public async Task<IActionResult> Delete(int conversationId) { var c=await _db.Conversations.FindAsync(conversationId); if(c==null||!await CanAccess(c)) return NotFound(); _conversations.Delete(c); await _conversations.SaveAsync(); return NoContent(); }
    [HttpPatch("{conversationId:int}/read")] public async Task<IActionResult> Read(int conversationId) { var c=await _db.Conversations.FindAsync(conversationId); if(c==null||!await CanAccess(c)) return NotFound(); var ms=await _messages.GetBySessionIdAsync(conversationId); foreach(var m in ms) if(m.SenderId!=UserId) m.IsRead=1; await _messages.SaveAsync(); return NoContent(); }
    [HttpGet("{conversationId:int}/messages")] public async Task<ActionResult<List<MessageDto>>> Messages(int conversationId) { var c=await _db.Conversations.FindAsync(conversationId); if(c==null||!await CanAccess(c)) return NotFound(); return Ok((await _messages.GetBySessionIdAsync(conversationId)).Select(MessageToDto)); }
    [HttpPost("{conversationId:int}/messages")] public async Task<ActionResult<MessageDto>> Send(int conversationId, [FromForm] SendMessageDto dto, IFormFile? file = null) { var c=await _db.Conversations.FindAsync(conversationId); if(c==null||!await CanAccess(c)) return NotFound(); if(file != null) { var uploaded=await _files.UploadMultipleAsync(new List<IFormFile>{file}, UserId); dto.FileId=uploaded.Single().FileId; } if(string.IsNullOrWhiteSpace(dto.Content)&&dto.FileId==null) return BadRequest("消息内容不能为空"); var next=(await _messages.GetBySessionIdAsync(conversationId)).Select(x=>x.MsgIndex).DefaultIfEmpty(0).Max()+1; var m=new Message{SessionId=conversationId,MsgIndex=next,SenderId=UserId,MsgType=dto.FileId.HasValue?(int)MessageType.Image:(int)MessageType.Text,FileId=dto.FileId,MsgContent=dto.Content?.Trim()??string.Empty}; await _messages.AddAsync(m); await _messages.SaveAsync(); return Ok(MessageToDto(m)); }
    [HttpPost("{conversationId:int}/attachments")] public async Task<ActionResult<MessageDto>> Attach(int conversationId, IFormFile file, [FromForm] string? content = null) => await Send(conversationId, new SendMessageDto { Content=content }, file);
    [HttpDelete("{conversationId:int}/messages/{messageId:int}")] public async Task<IActionResult> DeleteMessage(int conversationId,int messageId){var c=await _db.Conversations.FindAsync(conversationId);var m=await _messages.GetByIdAsync(conversationId,messageId);if(c==null||m==null||!await CanAccess(c)||m.SenderId!=UserId)return NotFound();_messages.Delete(m);await _messages.SaveAsync();return NoContent();}
}
