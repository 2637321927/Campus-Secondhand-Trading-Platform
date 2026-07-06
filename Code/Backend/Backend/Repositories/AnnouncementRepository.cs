using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class AnnouncementRepository : IAnnouncementRepository
{
    private readonly AppDbContext _context;
    public AnnouncementRepository(AppDbContext context) => _context = context;

    public async Task<Announcement?> GetByIdAsync(int announcementId)
        => await _context.Announcements.FindAsync(announcementId);

    public async Task<List<Announcement>> GetAllAsync()
        => await _context.Announcements.Include(a => a.Admin).ToListAsync();

    public async Task AddAsync(Announcement announcement)
        => await _context.Announcements.AddAsync(announcement);

    public void Update(Announcement announcement)
        => _context.Announcements.Update(announcement);

    public void Delete(Announcement announcement)
        => _context.Announcements.Remove(announcement);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
