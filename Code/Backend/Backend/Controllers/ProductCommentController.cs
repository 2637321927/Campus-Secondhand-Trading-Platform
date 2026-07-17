using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/products/{productId}/comments")]
public class ProductCommentController : ControllerBase
{
    private readonly IProductCommentService _commentService;

    public ProductCommentController(IProductCommentService commentService)
    {
        _commentService = commentService;
    }

    /// <summary>
    /// 根据商品 ID 获取评论列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductCommentDto>>> GetByProductId(long productId)
    {
        var comments = await _commentService.GetByProductIdAsync(productId);
        return Ok(comments);
    }

    /// <summary>
    /// 创建评论
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProductCommentDto>> Create(long productId, [FromBody] CreateProductCommentDto dto)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var comment = await _commentService.CreateAsync(productId, userId, dto);
        return CreatedAtAction(nameof(GetByProductId), new { productId }, comment);
    }

    /// <summary>
    /// 删除评论
    /// </summary>
    [Authorize]
    [HttpDelete("{commentId}")]
    public async Task<ActionResult> Delete(long productId, long commentId)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var result = await _commentService.DeleteAsync(commentId, userId);
        if (!result) return NotFound();
        return NoContent();
    }
}
