using Backend.Dtos.Home;
using Backend.Dtos.Product;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

///<summary>
///主页模块
///</summary>
[ApiController]
[Route("api/home")]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    ///<summary>
    ///获取首页聚合数据（推荐商品、分类入口、用户快捷入口）
    ///</summary>
    [HttpGet]
    public async Task<ActionResult<HomeResponseDto>> Index()
    {
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
            userId = int.Parse(User.FindFirst("userId")!.Value);

        var data = await _homeService.GetHomeDataAsync(userId);
        return Ok(data);
    }

    ///<summary>
    ///获取首页推荐商品列表
    ///</summary>
    [HttpGet("recommended-products")]
    public async Task<ActionResult<List<ProductCardDto>>> GetRecommended()
    {
        return Ok(await _homeService.GetRecommendedProductsAsync());
    }

    ///<summary>
    ///获取热门商品列表
    ///</summary>
    [HttpGet("hot-products")]
    public async Task<ActionResult<List<ProductCardDto>>> GetHotProducts()
    {
        return Ok(await _homeService.GetHotProductsAsync());
    }
}