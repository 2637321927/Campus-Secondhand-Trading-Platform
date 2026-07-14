using Backend.Dtos.Home;
using Backend.Dtos.Product;

namespace Backend.Services;

public interface IHomeService
{
    /// <summary>
    /// 获取首页聚合数据，userId 为 null 时不查用户快捷入口
    /// </summary>
    Task<HomeResponseDto> GetHomeDataAsync(int? userId);

    /// <summary>
    /// 获取首页推荐商品列表（最新 N 条）
    /// </summary>
    Task<List<ProductCardDto>> GetRecommendedProductsAsync();

    /// <summary>
    /// 获取热门商品列表（TODO：目前按发布时间排序）
    /// </summary>
    Task<List<ProductCardDto>> GetHotProductsAsync();
}