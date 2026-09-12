using Backend.Dtos.Communication;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;

    public NotificationService(INotificationRepository repo)
    {
        _repo = repo;
    }

    public async Task NotifyAsync(
        int userId,
        string title,
        string content,
        string type = "system",
        long? relatedId = null)
    {
        if (userId <= 0)
            return;

        await _repo.AddAsync(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Content = content,
            RelatedId = relatedId,
            IsRead = 0,
            CreateTime = DateTime.Now
        });
        await _repo.SaveAsync();
    }

    public async Task<List<NotificationDto>> GetByUserIdAsync(int userId)
    {
        var list = await _repo.GetByUserIdAsync(userId);
        return list.Select(Map).ToList();
    }

    public async Task<NotificationDto?> GetByIdAsync(long notificationId, int userId)
    {
        var n = await _repo.GetByIdAsync(notificationId);
        if (n == null || n.UserId != userId)
            return null;
        return Map(n);
    }

    public async Task<bool> MarkReadAsync(long notificationId, int userId)
    {
        var n = await _repo.GetByIdAsync(notificationId);
        if (n == null || n.UserId != userId)
            return false;

        if (n.IsRead == 0)
        {
            n.IsRead = 1;
            _repo.Update(n);
            await _repo.SaveAsync();
        }
        return true;
    }

    public async Task<bool> DeleteAsync(long notificationId, int userId)
    {
        var n = await _repo.GetByIdAsync(notificationId);
        if (n == null || n.UserId != userId)
            return false;

        _repo.Delete(n);
        await _repo.SaveAsync();
        return true;
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var list = await _repo.GetByUserIdAsync(userId);
        foreach (var n in list.Where(n => n.IsRead == 0))
        {
            n.IsRead = 1;
            _repo.Update(n);
        }
        await _repo.SaveAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
        => await _repo.GetUnreadCountAsync(userId);

    private static NotificationDto Map(Notification n) => new()
    {
        NotificationId = n.NotificationId,
        Type = n.Type,
        Title = n.Title,
        Content = n.Content,
        CreateTime = n.CreateTime,
        IsRead = n.IsRead == 1,
        RelatedId = n.RelatedId
    };
}
