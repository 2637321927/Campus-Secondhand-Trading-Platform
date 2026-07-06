using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class NormUserRepository : INormUserRepository
{
    private readonly AppDbContext _context;
    public NormUserRepository(AppDbContext context) => _context = context;

    public async Task<NormUser?> GetByIdAsync(int userId)
        => await _context.NormUsers.Include(n => n.BaseUser).FirstOrDefaultAsync(n => n.UserId == userId);

    public async Task<List<NormUser>> GetAllAsync()
        => await _context.NormUsers.Include(n => n.BaseUser).ToListAsync();

    public async Task AddAsync(NormUser user)
        => await _context.NormUsers.AddAsync(user);

    public void Update(NormUser user)
        => _context.NormUsers.Update(user);

    public void Delete(NormUser user)
        => _context.NormUsers.Remove(user);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
