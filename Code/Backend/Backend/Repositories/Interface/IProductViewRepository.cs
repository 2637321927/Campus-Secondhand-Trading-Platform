using Backend.Models;

namespace Backend.Repositories;

public interface IProductViewRepository
{
    Task AddAsync(ProductView view);
    Task<int> GetViewCountAsync(long productId);
    Task<Dictionary<long, int>> GetViewCountsAsync(IEnumerable<long> productIds);
    Task SaveAsync();
}
