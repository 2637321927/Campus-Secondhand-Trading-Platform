using Backend.Models;

namespace Backend.Repositories;

public interface ICollectionRepository
{
    Task<Collection?> GetByIdAsync(long productId, int userId);
    Task<List<Collection>> GetByUserIdAsync(int userId);
    Task<bool> IsCollectedAsync(long productId, int userId);
    Task AddAsync(Collection collection);
    void Delete(Collection collection);
    Task SaveAsync();
}
