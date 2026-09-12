using Backend.Models;

namespace Backend.Repositories;

public interface INotificationRepository
{
    IQueryable<Notification> Query();
    Task<Notification?> GetByIdAsync(long notificationId);
    Task<List<Notification>> GetByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task AddAsync(Notification notification);
    void Update(Notification notification);
    void Delete(Notification notification);
    Task SaveAsync();
}
