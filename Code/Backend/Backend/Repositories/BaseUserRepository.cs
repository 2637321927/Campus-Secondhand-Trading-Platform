using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class BaseUserRepository : IBaseUserRepository
{
    private readonly AppDbContext _context;
    public BaseUserRepository(AppDbContext context) => _context = context;

    public async Task<BaseUser?> GetByIdAsync(int userId)
        => await _context.BaseUsers.FindAsync(userId);

    public async Task<List<BaseUser>> GetAllAsync()
        => await _context.BaseUsers.ToListAsync();

    public async Task<BaseUser?> GetByEmailAsync(string email)
        => await _context.BaseUsers.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<BaseUser?> GetByPhoneAsync(string phone)
        => await _context.BaseUsers.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

    public async Task AddAsync(BaseUser user)
        => await _context.BaseUsers.AddAsync(user);

    public void Update(BaseUser user)
        => _context.BaseUsers.Update(user);

    public void Delete(BaseUser user)
        => _context.BaseUsers.Remove(user);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
