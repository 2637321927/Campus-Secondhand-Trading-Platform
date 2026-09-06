using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class ProductAuditLogRepository : IProductAuditLogRepository
{
    private readonly AppDbContext _context;

    public ProductAuditLogRepository(AppDbContext context) => _context = context;

    public IQueryable<ProductAuditLog> Query()
        => _context.ProductAuditLogs.AsQueryable();

    public async Task<List<ProductAuditLog>> GetByProductIdAsync(long productId)
        => await _context.ProductAuditLogs
            .Where(l => l.ProductId == productId)
            .Include(l => l.Admin)
            .OrderByDescending(l => l.CreateTime)
            .ToListAsync();

    public async Task AddAsync(ProductAuditLog log)
        => await _context.ProductAuditLogs.AddAsync(log);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
