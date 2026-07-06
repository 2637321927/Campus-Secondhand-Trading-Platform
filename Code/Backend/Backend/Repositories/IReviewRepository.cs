using Backend.Models;

namespace Backend.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int reviewId);
    Task<List<Review>> GetAllAsync();
    Task<Review?> GetByPurchaseIdAsync(long purchaseId);
    Task AddAsync(Review review);
    void Update(Review review);
    void Delete(Review review);
    Task SaveAsync();
}
