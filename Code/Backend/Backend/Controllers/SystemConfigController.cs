using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController, Route("api/system")]
public class SystemConfigController : ControllerBase
{
    [HttpGet("report-reasons")]
    public IActionResult ReportReasons() => Ok(new[]
    {
        new { code="fraud", name="欺诈或虚假信息", description="商品或用户信息与实际不符" },
        new { code="counterfeit", name="假冒伪劣", description="涉嫌售卖假冒、盗版或劣质商品" },
        new { code="illegal", name="违禁或违法内容", description="涉及法律法规禁止的内容" },
        new { code="harassment", name="骚扰或恶意行为", description="骚扰、辱骂、欺诈或恶意引流" },
        new { code="spam", name="垃圾信息", description="重复发布或无关推广信息" },
        new { code="other", name="其他", description="其他违规情况" }
    });

    [HttpGet("appeal-types")]
    public IActionResult AppealTypes() => Ok(new[]
    {
        new { code="product_removed", name="商品被下架", description="对商品下架处理提出申诉" },
        new { code="account_restricted", name="账号受限", description="对禁言、封禁或发布限制提出申诉" },
        new { code="report_result", name="举报处理结果", description="对举报处理结果提出异议" },
        new { code="other", name="其他", description="其他平台处理决定" }
    });
}
