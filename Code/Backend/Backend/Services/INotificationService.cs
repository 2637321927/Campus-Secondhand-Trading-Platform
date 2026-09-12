using Backend.Dtos.Communication;

namespace Backend.Services;

public interface INotificationService
{
    /// <summary>
    /// 给指定用户创建一条系统通知
    /// </summary>
    Task NotifyAsync(
        int userId,
        string title,
        string content,
        string type = "system",
        long? relatedId = null);

    Task<List<NotificationDto>> GetByUserIdAsync(int userId);
    Task<NotificationDto?> GetByIdAsync(long notificationId, int userId);
    Task<bool> MarkReadAsync(long notificationId, int userId);
    Task<bool> DeleteAsync(long notificationId, int userId);
    Task MarkAllReadAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
}
