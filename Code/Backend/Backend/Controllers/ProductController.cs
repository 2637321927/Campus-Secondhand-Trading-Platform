using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }
    
    /// <summary>
    /// 根据 ID 获取商品详情
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(long id)
    {
        var product = await _productService.GetByIdAsync(id);
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
        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);

    }

    /// <summary>
    /// 更新商品
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> Update(long id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);
        if (product == null) return NotFound();
        return Ok(product);
    }


    /// <summary>    /// 删除商品
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(long id)
    {

        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var result = await _productService.DeleteAsync(id, userId);
        if (!result) return NotFound();
        return NoContent();

    }

}
