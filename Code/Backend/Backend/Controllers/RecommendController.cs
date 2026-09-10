using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 推荐模块：首页"为你推荐" + 商品详情"猜你想看"
/// </summary>
[ApiController]
[Route("api/recommend")]
public class RecommendController : ControllerBase
{
    private readonly IRecommendService _recommendService;

    public RecommendController(IRecommendService recommendService)
    {
        _recommendService = recommendService;
    }

    /// <summary>
    /// 获取推荐商品（登录后按用户兴趣；未登录返回热度兜底）
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductCardDto>>> Recommend([FromQuery] int count = 10)
    {
        var userId = CurrentUserId();
        return Ok(await _recommendService.RecommendForUserAsync(userId, Math.Clamp(count, 1, 50)));
    }

    /// <summary>
    /// 商品详情"猜你想看"：同分类 + 用户偏好，返回 count 个
    /// </summary>
    [HttpGet("related/{productId:long}")]
    public async Task<ActionResult<List<ProductCardDto>>> Related(
        long productId,
        [FromQuery] int count = 4)
    {
        var userId = CurrentUserId();
        return Ok(await _recommendService.GetRelatedAsync(userId, productId, Math.Clamp(count, 1, 20)));
    }

    private int? CurrentUserId()
    {
        var claim = User?.FindFirst("userId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
