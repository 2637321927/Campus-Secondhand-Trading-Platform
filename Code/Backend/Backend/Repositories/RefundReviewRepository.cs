using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class RefundReviewRepository : IRefundReviewRepository
{
    private readonly AppDbContext _context;
    public RefundReviewRepository(AppDbContext context) => _context = context;

    public async Task<RefundReview?> GetByIdAsync(long refundId, int reviewerType)
        => await _context.RefundReviews.FindAsync(refundId, reviewerType);

    public async Task<List<RefundReview>> GetByRefundIdAsync(long refundId)
        => await _context.RefundReviews.Where(r => r.RefundId == refundId).ToListAsync();

    public async Task AddAsync(RefundReview review)
        => await _context.RefundReviews.AddAsync(review);

    public void Update(RefundReview review)
        => _context.RefundReviews.Update(review);

    public void Delete(RefundReview review)
        => _context.RefundReviews.Remove(review);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
