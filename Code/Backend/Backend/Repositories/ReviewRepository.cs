using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;
    public ReviewRepository(AppDbContext context) => _context = context;

    public async Task<Review?> GetByIdAsync(int reviewId)
        => await _context.Reviews
            .Include(r => r.Images)
            .Include(r => r.Purchase)
                .ThenInclude(p => p!.Product)
            .Include(r => r.Purchase!.Buyer)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

    public async Task<List<Review>> GetAllAsync()
        => await _context.Reviews
            .Include(r => r.Images)
            .ToListAsync();

    public async Task<Review?> GetByPurchaseIdAsync(long purchaseId)
        => await _context.Reviews
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.PurchaseId == purchaseId);

    public async Task<List<Review>> GetByProductIdAsync(long productId)
        => await _context.Reviews
            .Include(r => r.Images)
            .Include(r => r.Purchase)
                .ThenInclude(p => p!.Buyer)
            .Where(r => r.Purchase != null && r.Purchase.ProductId == productId)
            .OrderByDescending(r => r.ReviewTime)
            .ToListAsync();

    public async Task<List<Review>> GetByUserIdAsync(int userId)
        => await _context.Reviews
            .Include(r => r.Images)
            .Include(r => r.Purchase)
                .ThenInclude(p => p!.Product)
            .Where(r => r.Purchase != null && r.Purchase.BuyerId == userId)
            .OrderByDescending(r => r.ReviewTime)
            .ToListAsync();

    public async Task<List<Review>> GetReceivedReviewsAsync(int userId)
        => await _context.Reviews
            .Include(r => r.Images)
            .Include(r => r.Purchase)
                .ThenInclude(p => p!.Product)
            .Include(r => r.Purchase!.Buyer)
            .Where(r => r.Purchase != null && r.Purchase.Product != null
                && r.Purchase.Product.UserId == userId)
            .OrderByDescending(r => r.ReviewTime)
            .ToListAsync();

    public IQueryable<Review> Query()
        => _context.Reviews.AsQueryable();

    public async Task AddAsync(Review review)
        => await _context.Reviews.AddAsync(review);

    public void Update(Review review)
        => _context.Reviews.Update(review);

    public void Delete(Review review)
        => _context.Reviews.Remove(review);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
