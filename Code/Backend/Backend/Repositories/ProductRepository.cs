using Backend.Data;
using Backend.Models;
using Backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;
    public ProductRepository(AppDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(long productId)
        => await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Views)
            .FirstOrDefaultAsync(p => p.ProductId == productId);

    public async Task<List<Product>> GetAllAsync()
        => await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ToListAsync();

    public async Task<List<Product>> GetByCategoryAsync(long categoryId)
        => await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ToListAsync();

    public async Task<List<Product>> GetAvailableAsync()
        => await _context.Products
            .Where(p => p.Status == ProductStatus.Available)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ToListAsync();

    public async Task<(List<Product> Items, int Total)> GetAdminPageAsync(
        string? keyword,
        int? status,
        long? categoryId,
        int? sellerId,
        int page,
        int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(p =>
                p.Name.Contains(kw) ||
                (p.Info != null && p.Info.Contains(kw)) ||
                (p.Seller != null && p.Seller.UserName.Contains(kw)));
        }

        if (status.HasValue)
            query = query.Where(p => p.Status == (ProductStatus)status.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (sellerId.HasValue)
            query = query.Where(p => p.UserId == sellerId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.ReleaseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<Product>> GetByUserIdAsync(int userId)
        => await _context.Products
            .Where(p => p.UserId == userId)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ToListAsync();

    public async Task<List<Product>> GetSoldByUserIdAsync(int userId)
        => await _context.Products
            .Where(p => p.UserId == userId && p.Status == ProductStatus.Sold)
            .Include(p => p.Images)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .ToListAsync();

    public IQueryable<Product> Query()
        => _context.Products.AsQueryable();

    public async Task AddAsync(Product product)
        => await _context.Products.AddAsync(product);

    public void Update(Product product)
        => _context.Products.Update(product);

    public void Delete(Product product)
        => _context.Products.Remove(product);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
