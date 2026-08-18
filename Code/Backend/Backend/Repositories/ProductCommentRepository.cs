using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ProductCommentRepository : IProductCommentRepository
{
    private readonly AppDbContext _context;
    public ProductCommentRepository(AppDbContext context) => _context = context;

    public async Task<ProductComment?> GetByIdAsync(long commentId)
        => await _context.ProductComments.FindAsync(commentId);

    public async Task<List<ProductComment>> GetByProductIdAsync(long productId)
        => await _context.ProductComments
            .Where(c => c.ProductId == productId)
            .OrderBy(c => c.Index)
            .ToListAsync();

    public async Task<List<ProductComment>> GetRepliesAsync(long commentId)
        => await _context.ProductComments
            .Where(c => c.ResponseToId == commentId)
            .OrderBy(c => c.CreateTime)
            .ToListAsync();

    public async Task<int> GetNextIndexAsync(long productId)
        => await _context.ProductComments
            .Where(c => c.ProductId == productId)
            .MaxAsync(c => (int?)c.Index) is int maxIndex
            ? maxIndex + 1
            : 0;

    public async Task<bool> HasRepliesAsync(long commentId)
        => await _context.ProductComments
            .AnyAsync(c => c.ResponseToId == commentId);

    public async Task<Dictionary<long, int>> GetCountsByProductIdsAsync(IEnumerable<long> productIds)
        => await _context.ProductComments
            .Where(c => productIds.Contains(c.ProductId))
            .GroupBy(c => c.ProductId)
            .Select(g => new { ProductId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductId, x => x.Count);

    public async Task AddAsync(ProductComment comment)
        => await _context.ProductComments.AddAsync(comment);

    public void Delete(ProductComment comment)
        => _context.ProductComments.Remove(comment);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
