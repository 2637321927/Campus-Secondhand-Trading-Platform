using Backend.Data;
using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

/// <summary>
/// 会话模块 — 买家与卖家围绕某个商品的一对一聊天
/// 路由前缀：api/conversations
/// </summary>
[ApiController, Authorize, Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConversationRepository _conversations;
    private readonly IMessageRepository _messages;
    private readonly Backend.Services.IUpdatedFileService _files;

    public ConversationController(
        AppDbContext db,
        IConversationRepository conversations,
        IMessageRepository messages,
        Backend.Services.IUpdatedFileService files)
    {
        _db = db;
        _conversations = conversations;
        _messages = messages;
        _files = files;
    }

    private int UserId => int.Parse(User.FindFirst("userId")!.Value);

    /// <summary>
    /// 当前用户是否可访问该会话：是买家，或是会话关联商品的卖家
    /// </summary>
    private async Task<bool> CanAccess(Conversation c)
        => c.BuyerId == UserId
           || await _db.Products.AnyAsync(p => p.ProductId == c.ProductId && p.UserId == UserId);

    private static MessageDto MessageToDto(Message m) => new()
    {
        ConversationId = m.SessionId,
        MessageId = m.MsgIndex,
        SenderId = m.SenderId,
        MessageType = m.MsgType,
        FileId = m.FileId,
        Content = m.MsgContent,
        SendTime = m.SendTime,
        IsRead = m.IsRead == 1
    };

    private async Task<ConversationDto> ToDto(Conversation c)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == c.ProductId);
        var unread = await _db.Messages.CountAsync(m => m.SessionId == c.SessionId && m.SenderId != UserId && m.IsRead == 0);

        return new ConversationDto
        {
            ConversationId = c.SessionId,
            ProductId = c.ProductId,
            ProductName = product?.Name ?? string.Empty,
            BuyerId = c.BuyerId,
            SellerId = product?.UserId ?? 0,
            CreateTime = c.CreateTime,
            UnreadCount = unread
        };
    }

    // ==================== 会话 ====================

    /// <summary>
    /// 会话列表（按创建时间倒序），支持按商品名模糊搜索
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ConversationDto>>> List([FromQuery] string? keyword = null)
    {
        var ids = await _db.Conversations
            .Where(c => c.BuyerId == UserId
                        || _db.Products.Any(p => p.ProductId == c.ProductId && p.UserId == UserId))
            .Select(c => c.SessionId)
            .ToListAsync();

        var list = new List<ConversationDto>();
        foreach (var id in ids)
        {
            var c = await _db.Conversations.FindAsync(id);
            if (c == null) continue;

            var dto = await ToDto(c);
            if (string.IsNullOrWhiteSpace(keyword) || dto.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                list.Add(dto);
        }

        return Ok(list.OrderByDescending(x => x.CreateTime));
    }

    /// <summary>
    /// 创建（或复用已有的）与某商品卖家的会话
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConversationDto>> Create(CreateConversationDto dto)
    {
        var product = await _db.Products.FindAsync(dto.ProductId);
        if (product == null) return NotFound();
        if (product.UserId == UserId) return BadRequest("不能与自己商品创建会话");

        // 同一买家对同一商品只保留一个会话，已存在则直接返回
        var conversation = await _db.Conversations
            .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId && x.BuyerId == UserId);

        if (conversation == null)
        {
            conversation = new Conversation { ProductId = dto.ProductId, BuyerId = UserId };
            await _conversations.AddAsync(conversation);
            await _conversations.SaveAsync();
        }

        return Ok(await ToDto(conversation));
    }

    /// <summary>
    /// 获取单个会话详情
    /// </summary>
    [HttpGet("{conversationId:int}")]
    public async Task<ActionResult<ConversationDto>> Get(int conversationId)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        if (c == null || !await CanAccess(c)) return NotFound();

        return Ok(await ToDto(c));
    }

    /// <summary>
    /// 删除会话
    /// </summary>
    [HttpDelete("{conversationId:int}")]
    public async Task<IActionResult> Delete(int conversationId)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        if (c == null || !await CanAccess(c)) return NotFound();

        _conversations.Delete(c);
        await _conversations.SaveAsync();
        return NoContent();
    }

    /// <summary>
    /// 将会话中对方发来的消息全部标记为已读
    /// </summary>
    [HttpPatch("{conversationId:int}/read")]
    public async Task<IActionResult> Read(int conversationId)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        if (c == null || !await CanAccess(c)) return NotFound();

        var messages = await _messages.GetBySessionIdAsync(conversationId);
        foreach (var m in messages)
        {
            if (m.SenderId != UserId) m.IsRead = 1;
        }
        await _messages.SaveAsync();
        return NoContent();
    }

    // ==================== 消息 ====================

    /// <summary>
    /// 获取会话的全部消息记录
    /// </summary>
    [HttpGet("{conversationId:int}/messages")]
    public async Task<ActionResult<List<MessageDto>>> Messages(int conversationId)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        if (c == null || !await CanAccess(c)) return NotFound();

        return Ok((await _messages.GetBySessionIdAsync(conversationId)).Select(MessageToDto));
    }

    /// <summary>
    /// 发送文字消息（JSON 请求体）
    /// </summary>
    [HttpPost("{conversationId:int}/messages")]
    public async Task<ActionResult<MessageDto>> Send(int conversationId, [FromBody] SendMessageDto dto)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        if (c == null || !await CanAccess(c)) return NotFound();

        return await SendCore(conversationId, dto, file: null);
    }

    /// <summary>
    /// 发送附件消息（multipart 表单上传图片等文件）
    /// </summary>
    [HttpPost("{conversationId:int}/attachments")]
    public async Task<ActionResult<MessageDto>> Attach(int conversationId, IFormFile file, [FromForm] string? content = null)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        if (c == null || !await CanAccess(c)) return NotFound();

        return await SendCore(conversationId, new SendMessageDto { Content = content }, file);
    }

    /// <summary>
    /// 发送消息的公共实现：可带附件。
    /// 纯图片消息无文字时存占位符 "[图片]"（Oracle 空字符串视为 NULL，msg_content 列 NOT NULL）。
    /// </summary>
    private async Task<ActionResult<MessageDto>> SendCore(int conversationId, SendMessageDto dto, IFormFile? file)
    {
        // 附件：先上传拿到 FileId，再作为消息的一部分落库
        if (file != null)
        {
            var uploaded = await _files.UploadMultipleAsync(new List<IFormFile> { file }, UserId);
            dto.FileId = uploaded.Single().FileId;
        }

        if (string.IsNullOrWhiteSpace(dto.Content) && dto.FileId == null)
            return BadRequest("消息内容不能为空");

        // MsgIndex 为会话内自增序号：取当前最大值 +1
        var next = (await _messages.GetBySessionIdAsync(conversationId))
            .Select(x => x.MsgIndex)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var message = new Message
        {
            SessionId = conversationId,
            MsgIndex = next,
            SenderId = UserId,
            MsgType = dto.FileId.HasValue ? (int)MessageType.Image : (int)MessageType.Text,
            FileId = dto.FileId,
            MsgContent = string.IsNullOrWhiteSpace(dto.Content) ? "[图片]" : dto.Content.Trim()
        };

        await _messages.AddAsync(message);
        await _messages.SaveAsync();

        return Ok(MessageToDto(message));
    }

    /// <summary>
    /// 删除自己发送的一条消息
    /// </summary>
    [HttpDelete("{conversationId:int}/messages/{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(int conversationId, int messageId)
    {
        var c = await _db.Conversations.FindAsync(conversationId);
        var m = await _messages.GetByIdAsync(conversationId, messageId);
        if (c == null || m == null || !await CanAccess(c) || m.SenderId != UserId) return NotFound();

        _messages.Delete(m);
        await _messages.SaveAsync();
        return NoContent();
    }
}