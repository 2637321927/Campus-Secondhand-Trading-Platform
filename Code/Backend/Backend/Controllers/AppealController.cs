using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 申诉模块 — 用户对处罚/举报结果发起申诉（落到工单表 work_order，Type=Appeal）
/// </summary>
[ApiController, Authorize, Route("api/appeals")]
public class AppealController : ControllerBase
{
    private readonly IWorkOrderRepository _orders;
    private readonly IWorkOrderTimelineRepository _timeline;
    private readonly Backend.Services.IUpdatedFileService _files;

    public AppealController(
        IWorkOrderRepository orders,
        IWorkOrderTimelineRepository timeline,
        Backend.Services.IUpdatedFileService files)
    {
        _orders = orders;
        _timeline = timeline;
        _files = files;
    }

    private int Uid => int.Parse(User.FindFirst("userId")!.Value);

    private static WorkOrderDto Map(WorkOrder w) => new()
    {
        Id = w.WorkOrderId,
        Type = w.Type,
        Reason = w.Reason,
        Info = w.Info,
        Status = w.Status,
        Result = w.Result,
        Response = w.Response,
        CreateTime = w.CreateTime,
        TargetType = w.TargetType,
        TargetId = w.TargetId,
        AppealAgainstId = w.AppealAgainstWorkOrderId
    };

    /// <summary>
    /// 取属于自己的申诉工单：不存在 / 不是申诉类型 / 不属于当前用户时返回 null
    /// </summary>
    private async Task<WorkOrder?> Own(long id)
    {
        var w = await _orders.GetDetailAsync(id);
        return w is { Type: (int)WorkOrderType.Appeal, InitiatorId: var owner } && owner == Uid ? w : null;
    }

    // ==================== 申诉 CRUD ====================

    /// <summary>
    /// 发起申诉
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<WorkOrderDto>> Create(CreateAppealDto dto)
    {
        var workOrder = new WorkOrder
        {
            Type = (int)WorkOrderType.Appeal,
            InitiatorId = Uid,
            Reason = dto.Reason.Trim(),
            Info = dto.Info?.Trim(),
            AppealAgainstWorkOrderId = dto.AppealAgainstId,
            TargetType = dto.TargetType,
            TargetId = dto.TargetId
        };

        await _orders.AddAsync(workOrder);
        await _orders.SaveAsync();

        return Ok(Map(workOrder));
    }

    /// <summary>
    /// 我发起的申诉列表
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<List<WorkOrderDto>>> Mine()
        => Ok((await _orders.GetByInitiatorIdAsync(Uid))
            .Where(x => x.Type == (int)WorkOrderType.Appeal)
            .Select(Map));

    /// <summary>
    /// 申诉详情（仅发起人可见）
    /// </summary>
    [HttpGet("{appealId:long}")]
    public async Task<ActionResult<WorkOrderDto>> Get(long appealId)
    {
        var w = await Own(appealId);
        return w == null ? NotFound() : Ok(Map(w));
    }

    /// <summary>
    /// 追加补充说明：以"时间 + 用户补充"格式追加到工单 Info 字段
    /// </summary>
    [HttpPost("{appealId:long}/messages")]
    public async Task<ActionResult<WorkOrderDto>> Message(long appealId, AppendWorkOrderMessageDto dto)
    {
        var w = await Own(appealId);
        if (w == null) return NotFound();

        w.Info = ((w.Info ?? string.Empty) + $"\n{DateTime.Now:g} 用户补充：{dto.Message.Trim()}").Trim();
        _orders.Update(w);
        await _orders.SaveAsync();

        return Ok(Map(w));
    }

    /// <summary>
    /// 为申诉绑定附件：上传走文件模块拿到 fileId 后，将文件 ID/名追加到工单 Info 字段；
    /// 为兼容旧的直接上传方式，仍允许直接传 multipart 文件
    /// </summary>
    [HttpPost("{appealId:long}/attachments")]
    public async Task<ActionResult<WorkOrderDto>> Attachment(
        long appealId,
        [FromForm] long? fileId = null,
        IFormFile? file = null)
    {
        var w = await Own(appealId);
        if (w == null) return NotFound();

        if (fileId == null && (file == null || file.Length == 0))
            return BadRequest("附件不能为空");

        UpdatedFile f;
        if (fileId.HasValue)
        {
            var meta = await _files.GetActiveByIdAsync(fileId.Value);
            if (meta == null || meta.UploaderId != Uid)
                return BadRequest("附件不存在或不是当前用户上传");

            f = meta;
        }
        else
        {
            var uploaded = await _files.UploadMultipleAsync(new List<IFormFile> { file! }, Uid);
            f = uploaded.Single();
        }

        w.Info = ((w.Info ?? string.Empty) + $"\n[附件:{f.FileId}:{f.FileName}]").Trim();
        _orders.Update(w);
        await _orders.SaveAsync();

        return Ok(Map(w));
    }

    /// <summary>
    /// 撤销申诉（已处理的不能撤）
    /// </summary>
    [HttpPatch("{appealId:long}/cancel")]
    public async Task<IActionResult> Cancel(long appealId)
    {
        var w = await Own(appealId);
        if (w == null) return NotFound();

        if (w.Status == "done")
            return BadRequest("已处理的申诉不能撤销");

        _orders.Delete(w);
        await _orders.SaveAsync();
        return NoContent();
    }

    /// <summary>
    /// 申诉处理时间线
    /// </summary>
    [HttpGet("{appealId:long}/timeline")]
    public async Task<IActionResult> Timeline(long appealId)
    {
        var w = await Own(appealId);
        if (w == null) return NotFound();

        return Ok(await _timeline.GetByWorkOrderIdAsync(appealId));
    }
}
