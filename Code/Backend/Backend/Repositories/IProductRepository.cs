using Backend.Models;

namespace Backend.Repositories;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long productId);
    Task<List<Product>> GetByCategoryAsync(long categoryId);
    Task<List<Product>> GetByUserIdAsync(int userId);
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
    Task SaveAsync();
}
