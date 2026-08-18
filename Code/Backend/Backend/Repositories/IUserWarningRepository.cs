using Backend.Models;

namespace Backend.Repositories;

public interface IUserWarningRepository
{
    IQueryable<UserWarning> Query();
    Task<List<UserWarning>> GetRecentByUserIdAsync(int userId, int count);
    Task AddAsync(UserWarning warning);
    Task SaveAsync();
}
