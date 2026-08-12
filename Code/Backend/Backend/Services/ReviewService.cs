using Backend.Dtos.Review;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IProductRepository _productRepo;
    private readonly IBaseUserRepository _userRepo;

    public ReviewService(
        IReviewRepository reviewRepo,
        IPurchaseRepository purchaseRepo,
        IProductRepository productRepo,
        IBaseUserRepository userRepo)
    {
        _reviewRepo = reviewRepo;
        _purchaseRepo = purchaseRepo;
        _productRepo = productRepo;
        _userRepo = userRepo;
    }

    public async Task<ReviewDto> CreateReviewAsync(long orderId, int userId, CreateReviewDto dto)
    {
        var order = await _purchaseRepo.GetByIdAsync(orderId)
            ?? throw new ArgumentException("订单不存在");

        if (order.BuyerId != userId)
            throw new UnauthorizedAccessException("只有买家可以评价");

        if (order.Status != "success")
            throw new InvalidOperationException("只有已完成的订单可以评价");

        // 一个订单只能评价一次
        var existing = await _reviewRepo.GetByPurchaseIdAsync(orderId);
        if (existing != null)
            throw new InvalidOperationException("该订单已评价");

        if (dto.Rating < 1 || dto.Rating > 5)
            throw new ArgumentException("评分必须在1-5之间");

        var review = new Review
        {
            Rating = dto.Rating,
            Info = dto.Info,
            ReviewTime = DateTime.Now,
            PurchaseId = orderId
        };

        await _reviewRepo.AddAsync(review);
        await _reviewRepo.SaveAsync();

        return ToDto(review);

        // Note: Review images are linked via RevImage; we skip image handling here
        // since the current RevImage model references UpdatedFile
    }

    public async Task<ReviewDto?> GetOrderReviewAsync(long orderId)
    {
        var review = await _reviewRepo.GetByPurchaseIdAsync(orderId);
        return review == null ? null : ToDto(review);
    }

    public async Task<List<ReviewDto>> GetProductReviewsAsync(long productId)
    {
        var reviews = await _reviewRepo.GetByProductIdAsync(productId);
        return reviews.Where(r => r.IsHidden == 0).Select(ToDto).ToList();
    }

    public async Task<List<ReviewDto>> GetUserReceivedReviewsAsync(int userId)
    {
        var reviews = await _reviewRepo.GetReceivedReviewsAsync(userId);
        return reviews.Where(r => r.IsHidden == 0).Select(ToDto).ToList();
    }

    public async Task<List<ReviewDto>> GetUserGivenReviewsAsync(int userId)
    {
        var reviews = await _reviewRepo.GetByUserIdAsync(userId);
        return reviews.Where(r => r.IsHidden == 0).Select(ToDto).ToList();
    }

    public async Task<ReviewDto> ReplyReviewAsync(int reviewId, int userId, ReplyReviewDto dto)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId)
            ?? throw new ArgumentException("评价不存在");

        if (string.IsNullOrWhiteSpace(dto.ReplyInfo))
            throw new ArgumentException("回复内容不能为空");

        // 只有被评价方（卖家）可以回复
        if (review.Purchase == null || review.Purchase.Product == null)
            throw new InvalidOperationException("评价关联的订单或商品信息异常");

        if (review.Purchase.Product.UserId != userId)
            throw new UnauthorizedAccessException("只有被评价方（卖家）可以回复评价");

        if (review.ReplyInfo != null)
            throw new InvalidOperationException("已回复过该评价");

        review.ReplyInfo = dto.ReplyInfo;
        review.ReplyTime = DateTime.Now;
        _reviewRepo.Update(review);
        await _reviewRepo.SaveAsync();

        return ToDto(review);
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, int userId, bool isAdmin = false)
    {
        var review = await _reviewRepo.GetByIdAsync(reviewId);
        if (review == null)
            return false;

        // 评价人、被评价人、管理员都可以删除/屏蔽
        var isReviewer = review.Purchase?.BuyerId == userId;
        var isReviewee = review.Purchase?.Product?.UserId == userId;

        if (!isReviewer && !isReviewee && !isAdmin)
            throw new UnauthorizedAccessException("无权删除该评价");

        // 软删除：标记为隐藏
        review.IsHidden = 1;
        _reviewRepo.Update(review);
        await _reviewRepo.SaveAsync();

        return true;
    }

    // ==================== DTO 映射 ====================

    private static ReviewDto ToDto(Review r)
    {
        var buyerId = r.Purchase?.BuyerId ?? 0;
        var buyerName = r.Purchase?.Buyer?.UserName ?? "";
        var sellerId = r.Purchase?.Product?.UserId ?? 0;
        var sellerName = r.Purchase?.Product?.Seller?.UserName ?? "";

        return new ReviewDto
        {
            ReviewId = r.ReviewId,
            Rating = r.Rating,
            Info = r.Info,
            ReviewTime = r.ReviewTime,
            PurchaseId = r.PurchaseId,
            ReplyInfo = r.ReplyInfo,
            ReplyTime = r.ReplyTime,
            ReviewerId = buyerId,
            ReviewerName = buyerName,
            RevieweeId = sellerId,
            RevieweeName = sellerName,
            ProductId = r.Purchase?.ProductId ?? 0,
            ProductName = r.Purchase?.Product?.Name,
            IsHidden = r.IsHidden,
            Images = r.Images?.Select(i => new ReviewImageDto
            {
                ImgFileId = i.ImgFileId,
                ImgIndex = i.ImgIndex
            }).ToList() ?? new()
        };
    }
}
