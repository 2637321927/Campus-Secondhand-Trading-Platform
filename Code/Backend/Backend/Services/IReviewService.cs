using Backend.Dtos.Review;

namespace Backend.Services;

public interface IReviewService
{
    /// <summary>
    /// 对已完成订单进行评价
    /// </summary>
    Task<ReviewDto> CreateReviewAsync(long orderId, int userId, CreateReviewDto dto);

    /// <summary>
    /// 查看某订单评价
    /// </summary>
    Task<ReviewDto?> GetOrderReviewAsync(long orderId);

    /// <summary>
    /// 查看某商品相关评价
    /// </summary>
    Task<List<ReviewDto>> GetProductReviewsAsync(long productId);

    /// <summary>
    /// 查看某用户收到的评价
    /// </summary>
    Task<List<ReviewDto>> GetUserReceivedReviewsAsync(int userId);

    /// <summary>
    /// 查看某用户发出的评价
    /// </summary>
    Task<List<ReviewDto>> GetUserGivenReviewsAsync(int userId);

    /// <summary>
    /// 被评价方回复评价
    /// </summary>
    Task<ReviewDto> ReplyReviewAsync(int reviewId, int userId, ReplyReviewDto dto);

    /// <summary>
    /// 删除或屏蔽评价
    /// </summary>
    Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false);
}
