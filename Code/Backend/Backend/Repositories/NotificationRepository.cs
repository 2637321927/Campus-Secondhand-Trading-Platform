using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;
    public NotificationRepository(AppDbContext context) => _context = context;

    public IQueryable<Notification> Query()
        => _context.Notifications.AsQueryable();

    public async Task<Notification?> GetByIdAsync(long notificationId)
        => await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

    public async Task<List<Notification>> GetByUserIdAsync(int userId)
        => await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreateTime)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.IsRead == 0);

    public async Task AddAsync(Notification notification)
        => await _context.Notifications.AddAsync(notification);

    public void Update(Notification notification)
        => _context.Notifications.Update(notification);

    public void Delete(Notification notification)
        => _context.Notifications.Remove(notification);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
