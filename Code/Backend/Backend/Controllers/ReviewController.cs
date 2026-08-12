using Backend.Dtos.Review;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 评价模块
/// </summary>
[ApiController]
[Route("api")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// 对已完成订单进行评价
    /// </summary>
    [Authorize]
    [HttpPost("orders/{orderId}/reviews")]
    public async Task<ActionResult<ReviewDto>> CreateReview(long orderId, [FromBody] CreateReviewDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var review = await _reviewService.CreateReviewAsync(orderId, userId, dto);
            return CreatedAtAction(nameof(GetOrderReview), new { orderId }, review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查看某订单评价
    /// </summary>
    [HttpGet("orders/{orderId}/reviews")]
    public async Task<ActionResult<ReviewDto?>> GetOrderReview(long orderId)
    {
        var review = await _reviewService.GetOrderReviewAsync(orderId);
        if (review == null) return NotFound(new { error = "该订单暂无评价" });
        return Ok(review);
    }

    /// <summary>
    /// 查看某商品相关评价
    /// </summary>
    [HttpGet("products/{productId}/reviews")]
    public async Task<ActionResult<List<ReviewDto>>> GetProductReviews(long productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return Ok(reviews);
    }

    /// <summary>
    /// 查看某用户收到的评价
    /// </summary>
    [HttpGet("users/{userId}/reviews/received")]
    public async Task<ActionResult<List<ReviewDto>>> GetUserReceivedReviews(int userId)
    {
        var reviews = await _reviewService.GetUserReceivedReviewsAsync(userId);
        return Ok(reviews);
    }

    /// <summary>
    /// 查看某用户发出的评价
    /// </summary>
    [HttpGet("users/{userId}/reviews/given")]
    public async Task<ActionResult<List<ReviewDto>>> GetUserGivenReviews(int userId)
    {
        var reviews = await _reviewService.GetUserGivenReviewsAsync(userId);
        return Ok(reviews);
    }

    /// <summary>
    /// 被评价方回复评价
    /// </summary>
    [Authorize]
    [HttpPost("reviews/{reviewId}/reply")]
    public async Task<ActionResult<ReviewDto>> ReplyReview(int reviewId, [FromBody] ReplyReviewDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var review = await _reviewService.ReplyReviewAsync(reviewId, userId, dto);
            return Ok(review);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// 删除或屏蔽评价（支持本人或管理员权限）
    /// </summary>
    [Authorize]
    [HttpDelete("reviews/{reviewId}")]
    public async Task<ActionResult> DeleteReview(int reviewId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var userTypeClaim = User.FindFirst("userType");
            var isAdmin = userTypeClaim != null && int.Parse(userTypeClaim.Value) == 1;

            var result = await _reviewService.DeleteReviewAsync(reviewId, userId, isAdmin);
            if (!result) return NotFound(new { error = "评价不存在" });
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
