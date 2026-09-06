using Backend.Dtos.Reputation;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ReputationService : IReputationService
{
    private readonly IReviewRepository _reviewRepo;
    private readonly IPurchaseRepository _purchaseRepo;
    private readonly IBaseUserRepository _userRepo;
    private readonly INormUserRepository _normUserRepo;

    public ReputationService(
        IReviewRepository reviewRepo,
        IPurchaseRepository purchaseRepo,
        IBaseUserRepository userRepo,
        INormUserRepository normUserRepo)
    {
        _reviewRepo = reviewRepo;
        _purchaseRepo = purchaseRepo;
        _userRepo = userRepo;
        _normUserRepo = normUserRepo;
    }

    public async Task<ReputationSummaryDto> GetReputationSummaryAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        var normUser = await _normUserRepo.GetByIdAsync(userId);

        if (user == null || normUser == null)
            throw new ArgumentException("用户不存在");

        // 收到的评价（作为卖家）
        var receivedReviews = await _reviewRepo.GetReceivedReviewsAsync(userId);
        var visibleReviews = receivedReviews.Where(r => r.IsHidden == 0).ToList();

        int total = visibleReviews.Count;
        int good = visibleReviews.Count(r => r.Rating >= 4);
        int neutral = visibleReviews.Count(r => r.Rating == 3);
        int bad = visibleReviews.Count(r => r.Rating <= 2);
        double goodRate = total > 0 ? Math.Round((double)good / total * 100, 1) : 0;
        double avgRating = total > 0 ? Math.Round(visibleReviews.Average(r => r.Rating), 2) : 0;

        // 成交统计
        var allOrders = await _purchaseRepo.GetAllAsync();
        var completedSales = allOrders.Count(o =>
            o.Product != null && o.Product.UserId == userId && o.Status == "success");
        var completedPurchases = allOrders.Count(o =>
            o.BuyerId == userId && o.Status == "success");

        return new ReputationSummaryDto
        {
            UserId = userId,
            UserName = normUser.UserName,
            Credit = normUser.Credit,
            TotalReviews = total,
            GoodReviews = good,
            NeutralReviews = neutral,
            BadReviews = bad,
            GoodRate = goodRate,
            AverageRating = avgRating,
            CompletedSales = completedSales,
            CompletedPurchases = completedPurchases
        };
    }

    public async Task<ReputationDetailDto> GetReputationDetailAsync(int userId)
    {
        var summary = await GetReputationSummaryAsync(userId);

        // 收到的评价
        var receivedReviews = await _reviewRepo.GetReceivedReviewsAsync(userId);
        var visibleReviews = receivedReviews.Where(r => r.IsHidden == 0).ToList();

        // 近30天评价分布
        var thirtyDaysAgo = DateTime.Now.AddDays(-30);
        var recentReviews = visibleReviews.Where(r => r.ReviewTime >= thirtyDaysAgo).ToList();

        var ratingDistribution = new Dictionary<int, int>
        {
            { 5, recentReviews.Count(r => r.Rating == 5) },
            { 4, recentReviews.Count(r => r.Rating == 4) },
            { 3, recentReviews.Count(r => r.Rating == 3) },
            { 2, recentReviews.Count(r => r.Rating == 2) },
            { 1, recentReviews.Count(r => r.Rating == 1) }
        };

        // 违规记录（WorkOrder作为违规记录统计）
        // 由于WorkOrderRepository可能涉及复杂查询，这里做简化处理
        var allOrders = await _purchaseRepo.GetAllAsync();

        return new ReputationDetailDto
        {
            UserId = summary.UserId,
            UserName = summary.UserName,
            Credit = summary.Credit,
            TotalReviews = summary.TotalReviews,
            GoodReviews = summary.GoodReviews,
            NeutralReviews = summary.NeutralReviews,
            BadReviews = summary.BadReviews,
            GoodRate = summary.GoodRate,
            AverageRating = summary.AverageRating,
            CompletedSales = summary.CompletedSales,
            CompletedPurchases = summary.CompletedPurchases,
            RecentRatingDistribution = ratingDistribution,
            HasPendingViolation = false,  // TODO: 接入WorkOrder查询
            TotalWorkOrders = 0,          // TODO: 接入WorkOrder查询
            CompletedWorkOrders = 0       // TODO: 接入WorkOrder查询
        };
    }
}
