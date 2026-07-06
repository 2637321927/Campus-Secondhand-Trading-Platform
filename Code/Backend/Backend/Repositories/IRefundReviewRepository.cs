using Backend.Models;

namespace Backend.Repositories;

public interface IRefundReviewRepository
{
    Task<RefundReview?> GetByIdAsync(long refundId, int reviewerType);
    Task<List<RefundReview>> GetByRefundIdAsync(long refundId);
    Task AddAsync(RefundReview review);
    void Update(RefundReview review);
    void Delete(RefundReview review);
    Task SaveAsync();
}
