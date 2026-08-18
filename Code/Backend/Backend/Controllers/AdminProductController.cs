using Backend.Dtos.Admin;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 管理员商品管理模块
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/products")]
public class AdminProductController : ControllerBase
{
    private readonly IAdminProductManagementService _service;

    public AdminProductController(IAdminProductManagementService service)
    {
        _service = service;
    }

    /// <summary>
    /// 管理员查询全平台商品列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<AdminProductPageDto>> GetProducts(
        [FromQuery] string? keyword = null,
        [FromQuery] int? status = null,
        [FromQuery] long? categoryId = null,
        [FromQuery] int? sellerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _service.GetProductsAsync(keyword, status, categoryId, sellerId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// 获取待审核商品列表
    /// </summary>
    [HttpGet("pending-review")]
    public async Task<ActionResult<AdminProductPageDto>> GetPendingReview(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _service.GetPendingReviewAsync(page, pageSize));
    }

    /// <summary>
    /// 获取商品审核统计
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<AdminProductStatisticsDto>> GetStatistics()
    {
        return Ok(await _service.GetStatisticsAsync());
    }

    /// <summary>
    /// 管理员查看商品详情
    /// </summary>
    [HttpGet("{productId:long}")]
    public async Task<ActionResult<AdminProductDetailDto>> GetProduct(long productId)
    {
        var product = await _service.GetProductDetailAsync(productId);
        if (product == null) return NotFound(new { error = "商品不存在" });
        return Ok(product);
    }

    /// <summary>
    /// 审核通过商品
    /// </summary>
    [HttpPatch("{productId:long}/approve")]
    public async Task<ActionResult<AdminProductDetailDto>> Approve(long productId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.ApproveAsync(productId, adminId));
    }

    /// <summary>
    /// 审核驳回商品
    /// </summary>
    [HttpPatch("{productId:long}/reject")]
    public async Task<ActionResult<AdminProductDetailDto>> Reject(
        long productId,
        [FromBody] RejectProductDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.RejectAsync(productId, dto, adminId));
    }

    /// <summary>
    /// 强制下架商品
    /// </summary>
    [HttpPatch("{productId:long}/remove")]
    public async Task<ActionResult<AdminProductDetailDto>> Remove(
        long productId,
        [FromBody] RemoveProductDto dto)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.RemoveAsync(productId, dto, adminId));
    }

    /// <summary>
    /// 恢复被下架商品
    /// </summary>
    [HttpPatch("{productId:long}/restore")]
    public async Task<ActionResult<AdminProductDetailDto>> Restore(long productId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);
        return await RunActionAsync(() => _service.RestoreAsync(productId, adminId));
    }

    /// <summary>
    /// 管理员删除严重违规商品
    /// </summary>
    [HttpDelete("{productId:long}")]
    public async Task<IActionResult> Delete(long productId)
    {
        var adminId = int.Parse(User.FindFirst("userId")!.Value);

        try
        {
            var deleted = await _service.DeleteAsync(productId, adminId);
            if (!deleted) return NotFound(new { error = "商品不存在" });
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查看商品审核和管理记录
    /// </summary>
    [HttpGet("{productId:long}/audit-logs")]
    public async Task<ActionResult<List<AdminProductAuditLogDto>>> GetAuditLogs(long productId)
    {
        var logs = await _service.GetAuditLogsAsync(productId);
        if (logs == null) return NotFound(new { error = "商品不存在" });
        return Ok(logs);
    }

    private async Task<ActionResult<AdminProductDetailDto>> RunActionAsync(
        Func<Task<AdminProductDetailDto?>> action)
    {
        try
        {
            var product = await action();
            if (product == null) return NotFound(new { error = "商品不存在" });
            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
