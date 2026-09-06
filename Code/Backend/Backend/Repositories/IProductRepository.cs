using Backend.Models;

namespace Backend.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long productId);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByCategoryAsync(long categoryId);
    Task<List<Product>> GetByUserIdAsync(int userId);
    Task<List<Product>> GetSoldByUserIdAsync(int userId);
    Task<List<Product>> GetAvailableAsync();
    Task<(List<Product> Items, int Total)> GetAdminPageAsync(
        string? keyword,
        int? status,
        long? categoryId,
        int? sellerId,
        int page,
        int pageSize);
    IQueryable<Product> Query();
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task SaveAsync();
}
