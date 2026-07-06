using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AppDbContext _context;
    public AdminUserRepository(AppDbContext context) => _context = context;

    public async Task<AdminUser?> GetByIdAsync(int userId)
        => await _context.AdminUsers.Include(a => a.BaseUser).FirstOrDefaultAsync(a => a.UserId == userId);

    public async Task<List<AdminUser>> GetAllAsync()
        => await _context.AdminUsers.Include(a => a.BaseUser).ToListAsync();

    public async Task AddAsync(AdminUser user)
        => await _context.AdminUsers.AddAsync(user);

    public void Update(AdminUser user)
        => _context.AdminUsers.Update(user);

    public void Delete(AdminUser user)
        => _context.AdminUsers.Remove(user);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
