using Backend.Dtos.Product;

namespace Backend.Services;

public interface ICollectionService
{
    ///<summary>
    ///收藏/取消收藏（Toggle）
    ///</summary>
    ///<returns>
    ///true = 已收藏，false = 已取消
    ///</returns>
    Task<bool> ToggleAsync(long productId, int userId);

    ///<summary>
    ///查询是否已收藏
    ///</summary>
    Task<bool> IsCollectedAsync(long productId, int userId);

    ///<summary>
    ///获取我的收藏列表
    ///</summary>
    Task<List<ProductCardDto>> GetMyCollectionsAsync(int userId);

    ///<summary>
    ///获取收藏总数
    ///</summary>
    Task<int> GetCollectionCountAsync(int userId);

    ///<summary>
    ///批量取消收藏，返回成功删除的数量
    ///</summary>
    Task<int> BatchDeleteAsync(int userId, List<long> productIds);

    ///<summary>
    ///在收藏列表内搜索商品（按名称模糊匹配）TODO：目前只能搜连续的关键字，不能分开搜
    ///</summary>
    ///<returns>
    ///搜索结果列表，按收藏时间倒序排列
    ///</returns>
    Task<List<ProductCardDto>> SearchAsync(int userId, string keyword);
}
