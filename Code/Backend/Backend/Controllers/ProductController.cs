using Backend.Dtos.Product;
using Backend.Models.Enums;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IProdImageService _prodImageService;

    public ProductController(IProductService productService, IProdImageService prodImageService)
    {
        _productService = productService;
        _prodImageService = prodImageService;
    }

    /// <summary>
    /// 获取商品列表（按状态筛选）
    /// status：缺省或 0 = 在售；1 = 已售；-1 = 在售+已售
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll([FromQuery] int? status = null)
    {
        var statuses = status switch
        {
            -1 => new[] { ProductStatus.Available, ProductStatus.Sold },
            1 => new[] { ProductStatus.Sold },
            _ => new[] { ProductStatus.Available }
        };

        return Ok(await _productService.GetByStatusesAsync(statuses));
    }

    /// <summary>
    /// 获取指定用户发布的全部商品 ID
    /// </summary>
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<List<long>>> GetProductIdsByUserId(int userId)
    {
        if (userId <= 0)
            return BadRequest(new { error = "userId must be greater than zero." });

        var productIds = await _productService
            .GetProductIdsByUserIdAsync(userId);

        return Ok(productIds);
    }
    
    /// <summary>
    /// 根据 ID 获取商品详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        
        var userIdClaim = User.FindFirst("userId");
        var userId = -1;
        if (userIdClaim != null) userId = int.Parse(userIdClaim.Value);
        var product = await _productService.GetByIdAsync(id, userId);
        if (product == null) return NotFound();
        return Ok(product);

    }

    /// <summary>
    /// 记录商品浏览
    /// </summary>
    [Authorize]
    [HttpPost("{id}/view-record")]
    public async Task<ActionResult> RecordView(long id)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        await _productService.RecordViewAsync(id, userId);
        return NoContent();
    }

    /// <summary>
    /// 发布商品
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromForm] CreateProductDto dto)
    {

        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var product = await _productService.CreateAsync(userId, dto);
        return CreatedAtAction(nameof(GetById), new { id = product!.ProductId }, product);

    }

    /// <summary>
    /// 更新商品
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Update(long id, [FromForm] UpdateProductDto dto)
    {

        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var product = await _productService.UpdateAsync(id, userId, dto);
        if (product == null) return NotFound();
        return Ok(product);

    }

    /// <summary>
    /// /// 删除商品
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(long id)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var result = await _productService.DeleteAsync(id, userId);
            if (!result) return NotFound();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }

    }

    /// <summary>
    /// 修改商品状态（在售/已售/已下架之间的流转）
    /// </summary>
    [HttpPatch("{id}/status")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> UpdateStatus(long id, [FromBody] UpdateProductStatusDto dto)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")!.Value);
            var product = await _productService.UpdateStatusAsync(id, userId, dto.Status);
            if (product == null) return NotFound();
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
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }

    /// <summary>
    /// 批量获取商品图片
    /// </summary>
    [HttpPost("images")]
    public async Task<ActionResult<List<ProductImageDataDto>>> GetImages([FromBody] List<long> fileIds)
    {

        if (fileIds == null || fileIds.Count == 0)
            return BadRequest("fileIds is required.");

        var images = await _prodImageService.GetProductImagesAsync(fileIds);
        return Ok(images);

    }

}
