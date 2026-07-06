using Backend.Data;
using Backend.Models;
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
            .FirstOrDefaultAsync(p => p.ProductId == productId);

    public async Task<List<Product>> GetAllAsync()
        => await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Category)
            .ToListAsync();

    public async Task<List<Product>> GetByCategoryAsync(long categoryId)
        => await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Images)
            .ToListAsync();

    public async Task<List<Product>> GetByUserIdAsync(int userId)
        => await _context.Products
            .Where(p => p.UserId == userId)
            .Include(p => p.Images)
            .ToListAsync();

    public async Task<List<Product>> GetAvailableAsync()
        => await _context.Products
            .Where(p => p.Status == "available")
            .Include(p => p.Images)
            .Include(p => p.Category)
            .ToListAsync();

    public async Task AddAsync(Product product)
        => await _context.Products.AddAsync(product);

    public void Update(Product product)
        => _context.Products.Update(product);

    public void Delete(Product product)
        => _context.Products.Remove(product);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
