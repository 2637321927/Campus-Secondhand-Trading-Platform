using Backend.Models;

namespace Backend.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long productId);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetByCategoryAsync(int categoryId);
    Task<List<Product>> GetByUserIdAsync(int userId);
    Task<List<Product>> GetAvailableAsync();
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task SaveAsync();
}
