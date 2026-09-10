using Backend.Dtos.Product;

namespace Backend.Services;

public interface IRecommendService
{
    /// <summary>
    /// 按用户兴趣（浏览/收藏/成交 → 分类偏好）推荐商品；无 userId 或行为太少时退回热度兜底。
    /// </summary>
    Task<List<ProductCardDto>> RecommendForUserAsync(int? userId, int count);

    /// <summary>
    /// 商品详情页"猜你想看"：同分类商品为主，叠加该用户历史偏好权重。
    /// </summary>
    Task<List<ProductCardDto>> GetRelatedAsync(int? userId, long productId, int count);
}
