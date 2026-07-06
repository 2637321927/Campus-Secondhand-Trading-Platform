using Backend.Models;

namespace Backend.Repositories;

public interface IBaseUserRepository
{
    Task<BaseUser?> GetByIdAsync(int userId);
    Task<List<BaseUser>> GetAllAsync();
    Task<BaseUser?> GetByEmailAsync(string email);
    Task AddAsync(BaseUser user);
    void Update(BaseUser user);
    void Delete(BaseUser user);
    Task SaveAsync();
}
