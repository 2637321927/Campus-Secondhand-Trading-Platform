using Backend.Dtos.Product;
using Backend.Services;
using Backend.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
public class ShareController : ControllerBase
{
    private readonly IProductService _productService;

    public ShareController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// 生成商品分享码
    /// </summary>
    [HttpGet("api/products/{productId}/share-link")]
    public async Task<ActionResult> GenerateShareLink(long productId)
    {
        var product = await _productService.GetByIdAsync(productId, -1);
        if (product == null) return NotFound();

        var code = ShareCodeHelper.Encode(productId);
        return Ok(new { code });
    }

    /// <summary>
    /// 通过分享码访问商品详情
    /// </summary>
    [HttpGet("api/share/products/{code}")]
    public async Task<ActionResult<ProductDto>> GetByShareCode(string code)
    {
        long productId;
        try { productId = ShareCodeHelper.Decode(code); }
        catch { return NotFound(); }

        var product = await _productService.GetByIdAsync(productId, -1);
        if (product == null) return NotFound();

        return Ok(product);
    }
}
