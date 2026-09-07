using Backend.Models;

namespace Backend.Repositories;

public interface IProductAuditLogRepository
{
    IQueryable<ProductAuditLog> Query();
    Task<List<ProductAuditLog>> GetByProductIdAsync(long productId);
    Task AddAsync(ProductAuditLog log);
    Task SaveAsync();
}
