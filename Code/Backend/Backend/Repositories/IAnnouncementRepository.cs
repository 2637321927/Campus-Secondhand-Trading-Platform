using Backend.Models;

namespace Backend.Repositories;

public interface IAnnouncementRepository
{
    Task<Announcement?> GetByIdAsync(int announcementId);
    Task<List<Announcement>> GetAllAsync();
    Task AddAsync(Announcement announcement);
    void Update(Announcement announcement);
    void Delete(Announcement announcement);
    Task SaveAsync();
}
