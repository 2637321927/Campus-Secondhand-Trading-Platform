using Backend.Data;
using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController, Authorize, Route("api")]
public class ReportController : ControllerBase
{
    private readonly AppDbContext _db; private readonly IWorkOrderRepository _orders; private readonly IWorkOrderTimelineRepository _timeline; private readonly Backend.Services.IUpdatedFileService _files;
    public ReportController(AppDbContext db,IWorkOrderRepository orders,IWorkOrderTimelineRepository timeline, Backend.Services.IUpdatedFileService files){_db=db;_orders=orders;_timeline=timeline;_files=files;}
    private int Uid=>int.Parse(User.FindFirst("userId")!.Value);
    private static WorkOrderDto Map(WorkOrder w)=>new(){Id=w.WorkOrderId,Type=w.Type,Reason=w.Reason,Info=w.Info,Status=w.Status,Result=w.Result,Response=w.Response,CreateTime=w.CreateTime,TargetType=w.TargetType,TargetId=w.TargetId,AppealAgainstId=w.AppealAgainstWorkOrderId};
    [HttpGet("report-reasons")] public IActionResult Reasons()=>Ok(new[]{new{code="fraud",name="欺诈或虚假信息"},new{code="illegal",name="违禁或违法内容"},new{code="spam",name="骚扰或垃圾信息"},new{code="other",name="其他"}});
    [HttpPost("reports")] public async Task<ActionResult<WorkOrderDto>> Create(CreateReportDto dto){if(string.IsNullOrWhiteSpace(dto.TargetType)||dto.TargetId<=0)return BadRequest();var w=new WorkOrder{Type=(int)WorkOrderType.Report,InitiatorId=Uid,AccusedId=dto.AccusedId,ProductId=dto.ProductId,TargetType=dto.TargetType.Trim(),TargetId=dto.TargetId,Reason=dto.Reason.Trim(),Info=dto.Info?.Trim()};await _orders.AddAsync(w);await _orders.SaveAsync();return Ok(Map(w));}
    [HttpGet("reports/me")] public async Task<ActionResult<List<WorkOrderDto>>> Mine()=>Ok((await _orders.GetByInitiatorIdAsync(Uid)).Where(x=>x.Type==(int)WorkOrderType.Report).Select(Map));
    [HttpGet("reports/{reportId:long}")] public async Task<ActionResult<WorkOrderDto>> Get(long reportId){var w=await _orders.GetDetailAsync(reportId);return w==null||w.Type!=(int)WorkOrderType.Report||w.InitiatorId!=Uid?NotFound():Ok(Map(w));}
    [HttpPatch("reports/{reportId:long}/cancel")] public async Task<IActionResult> Cancel(long reportId){var w=await _orders.GetByIdAsync(reportId);if(w==null||w.Type!=(int)WorkOrderType.Report||w.InitiatorId!=Uid)return NotFound();if(w.Status=="done")return BadRequest("已处理的举报不能撤销");_orders.Delete(w);await _orders.SaveAsync();return NoContent();}
    [HttpPost("reports/{reportId:long}/attachments")] public async Task<ActionResult<WorkOrderDto>> Attachment(long reportId,IFormFile file){var w=await _orders.GetByIdAsync(reportId);if(w==null||w.Type!=(int)WorkOrderType.Report||w.InitiatorId!=Uid)return NotFound();if(file==null||file.Length==0)return BadRequest("附件不能为空");var uploaded=await _files.UploadMultipleAsync(new List<IFormFile>{file},Uid);w.Info=((w.Info??string.Empty)+$"\n[附件:{uploaded.Single().FileId}:{uploaded.Single().FileName}]").Trim();_orders.Update(w);await _orders.SaveAsync();return Ok(Map(w));}
    [HttpGet("products/{productId:long}/report-info")] public async Task<IActionResult> ProductInfo(long productId){var p=await _db.Products.AsNoTracking().FirstOrDefaultAsync(x=>x.ProductId==productId);return p==null?NotFound():Ok(new{productId=p.ProductId,name=p.Name,sellerId=p.UserId,status=p.Status.ToString()});}
    [HttpGet("users/{userId:int}/report-info")] public async Task<IActionResult> UserInfo(int userId){var u=await _db.NormUsers.AsNoTracking().FirstOrDefaultAsync(x=>x.UserId==userId);return u==null?NotFound():Ok(new{userId=u.UserId,userName=u.UserName,profile=u.Profile});}
}
