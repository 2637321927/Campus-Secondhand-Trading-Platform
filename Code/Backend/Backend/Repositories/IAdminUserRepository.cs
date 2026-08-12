using Backend.Models;

namespace Backend.Repositories;

public interface IAdminUserRepository
{
    Task<AdminUser?> GetByIdAsync(int userId);
    Task<List<AdminUser>> GetAllAsync();
    Task AddAsync(AdminUser user);
    void Update(AdminUser user);
    void Delete(AdminUser user);
    Task SaveAsync();
}
