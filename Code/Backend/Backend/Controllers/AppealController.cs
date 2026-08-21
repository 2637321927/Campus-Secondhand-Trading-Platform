using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController, Authorize, Route("api/appeals")]
public class AppealController : ControllerBase
{
    private readonly IWorkOrderRepository _orders; private readonly IWorkOrderTimelineRepository _timeline; private readonly Backend.Services.IUpdatedFileService _files;
    public AppealController(IWorkOrderRepository orders,IWorkOrderTimelineRepository timeline, Backend.Services.IUpdatedFileService files){_orders=orders;_timeline=timeline;_files=files;}
    private int Uid=>int.Parse(User.FindFirst("userId")!.Value);
    private static WorkOrderDto Map(WorkOrder w)=>new(){Id=w.WorkOrderId,Type=w.Type,Reason=w.Reason,Info=w.Info,Status=w.Status,Result=w.Result,Response=w.Response,CreateTime=w.CreateTime,TargetType=w.TargetType,TargetId=w.TargetId,AppealAgainstId=w.AppealAgainstWorkOrderId};
    private async Task<WorkOrder?> Own(long id)=> (await _orders.GetDetailAsync(id)) is {Type:(int)WorkOrderType.Appeal,InitiatorId:var owner} w&&owner==Uid?w:null;
    [HttpPost] public async Task<ActionResult<WorkOrderDto>> Create(CreateAppealDto dto){var w=new WorkOrder{Type=(int)WorkOrderType.Appeal,InitiatorId=Uid,Reason=dto.Reason.Trim(),Info=dto.Info?.Trim(),AppealAgainstWorkOrderId=dto.AppealAgainstId,TargetType=dto.TargetType,TargetId=dto.TargetId};await _orders.AddAsync(w);await _orders.SaveAsync();return Ok(Map(w));}
    [HttpGet("me")] public async Task<ActionResult<List<WorkOrderDto>>> Mine()=>Ok((await _orders.GetByInitiatorIdAsync(Uid)).Where(x=>x.Type==(int)WorkOrderType.Appeal).Select(Map));
    [HttpGet("{appealId:long}")] public async Task<ActionResult<WorkOrderDto>> Get(long appealId){var w=await Own(appealId);return w==null?NotFound():Ok(Map(w));}
    [HttpPost("{appealId:long}/messages")] public async Task<ActionResult<WorkOrderDto>> Message(long appealId,AppendWorkOrderMessageDto dto){var w=await Own(appealId);if(w==null)return NotFound();w.Info=((w.Info??string.Empty)+$"\n{DateTime.Now:g} 用户补充：{dto.Message.Trim()}").Trim();_orders.Update(w);await _orders.SaveAsync();return Ok(Map(w));}
    [HttpPost("{appealId:long}/attachments")] public async Task<ActionResult<WorkOrderDto>> Attachment(long appealId,[FromForm] IFormFile file){var w=await Own(appealId);if(w==null)return NotFound();if(file==null||file.Length==0)return BadRequest("附件不能为空");var uploaded=await _files.UploadMultipleAsync(new List<IFormFile>{file},Uid);w.Info=((w.Info??string.Empty)+$"\n[附件:{uploaded.Single().FileId}:{uploaded.Single().FileName}]").Trim();_orders.Update(w);await _orders.SaveAsync();return Ok(Map(w));}
    [HttpPatch("{appealId:long}/cancel")] public async Task<IActionResult> Cancel(long appealId){var w=await Own(appealId);if(w==null)return NotFound();if(w.Status=="done")return BadRequest("已处理的申诉不能撤销");_orders.Delete(w);await _orders.SaveAsync();return NoContent();}
    [HttpGet("{appealId:long}/timeline")] public async Task<IActionResult> Timeline(long appealId){var w=await Own(appealId);if(w==null)return NotFound();return Ok(await _timeline.GetByWorkOrderIdAsync(appealId));}
}
