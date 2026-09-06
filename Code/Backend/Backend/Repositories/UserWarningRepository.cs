using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class UserWarningRepository : IUserWarningRepository
{
    private readonly AppDbContext _context;

    public UserWarningRepository(AppDbContext context) => _context = context;

    public IQueryable<UserWarning> Query()
        => _context.UserWarnings.AsQueryable();

    public async Task<List<UserWarning>> GetRecentByUserIdAsync(int userId, int count)
        => await _context.UserWarnings
            .Where(w => w.UserId == userId)
            .Include(w => w.Admin)
            .OrderByDescending(w => w.CreateTime)
            .Take(count)
            .ToListAsync();

    public async Task AddAsync(UserWarning warning)
        => await _context.UserWarnings.AddAsync(warning);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
