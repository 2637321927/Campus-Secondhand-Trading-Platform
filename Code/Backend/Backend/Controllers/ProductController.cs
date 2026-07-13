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
    /// 发布商品
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var product = await _productService.CreateAsync(dto,userId);
        return CreatedAtAction(nameof(GetById), new { id = product.ProductId }, product);
    }

    /// <summary>
    /// 更新商品
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> Update(long id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);
        if (product == null) return NotFound();
        return Ok(product);
    }

    /// <summary>
    /// 删除商品
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(long id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
