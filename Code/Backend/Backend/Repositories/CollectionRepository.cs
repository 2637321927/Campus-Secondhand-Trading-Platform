using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class CollectionRepository : ICollectionRepository
{
    private readonly AppDbContext _context;
    public CollectionRepository(AppDbContext context) => _context = context;

    public async Task<Collection?> GetByIdAsync(long productId, int userId)
        => await _context.Collections.FindAsync(productId, userId);

    public async Task<List<Collection>> GetByUserIdAsync(int userId)
        => await _context.Collections
            .Where(c => c.UserId == userId)
            .Include(c => c.Product)
                .ThenInclude(p => p.Images)
            .Include(c => c.Product)
                .ThenInclude(p => p.Seller)
            .ToListAsync();

    public async Task<bool> IsCollectedAsync(long productId, int userId)
        => await _context.Collections.AnyAsync(c => c.ProductId == productId && c.UserId == userId);

    public async Task AddAsync(Collection collection)
        => await _context.Collections.AddAsync(collection);

    public void Delete(Collection collection)
        => _context.Collections.Remove(collection);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
