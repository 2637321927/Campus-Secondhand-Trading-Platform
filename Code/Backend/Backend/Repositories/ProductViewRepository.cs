using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ProductViewRepository : IProductViewRepository
{
    private readonly AppDbContext _context;
    public ProductViewRepository(AppDbContext context) => _context = context;

    public async Task AddAsync(ProductView view)
        => await _context.ProductViews.AddAsync(view);

    public async Task<int> GetViewCountAsync(long productId)
        => await _context.ProductViews.CountAsync(v => v.ProductId == productId);

    public async Task<Dictionary<long, int>> GetViewCountsAsync(IEnumerable<long> productIds)
        => await _context.ProductViews
            .Where(v => productIds.Contains(v.ProductId))
            .GroupBy(v => v.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
