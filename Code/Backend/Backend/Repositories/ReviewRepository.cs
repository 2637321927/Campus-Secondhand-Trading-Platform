using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;
    public ReviewRepository(AppDbContext context) => _context = context;

    public async Task<Review?> GetByIdAsync(int reviewId)
        => await _context.Reviews.Include(r => r.Images).FirstOrDefaultAsync(r => r.ReviewId == reviewId);

    public async Task<List<Review>> GetAllAsync()
        => await _context.Reviews.Include(r => r.Images).ToListAsync();

    public async Task<Review?> GetByPurchaseIdAsync(long purchaseId)
        => await _context.Reviews.Include(r => r.Images).FirstOrDefaultAsync(r => r.PurchaseId == purchaseId);

    public async Task AddAsync(Review review)
        => await _context.Reviews.AddAsync(review);

    public void Update(Review review)
        => _context.Reviews.Update(review);

    public void Delete(Review review)
        => _context.Reviews.Remove(review);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
