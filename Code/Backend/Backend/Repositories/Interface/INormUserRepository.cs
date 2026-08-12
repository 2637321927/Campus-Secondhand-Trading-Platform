using Backend.Models;

namespace Backend.Repositories;

public interface INormUserRepository
{
    Task<NormUser?> GetByIdAsync(int userId);
    Task<List<NormUser>> GetAllAsync();
    Task AddAsync(NormUser user);
    void Update(NormUser user);
    void Delete(NormUser user);
    Task SaveAsync();
}
