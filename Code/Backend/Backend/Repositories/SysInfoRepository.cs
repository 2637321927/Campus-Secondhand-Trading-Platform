using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class SysInfoRepository : ISysInfoRepository
{
    private readonly AppDbContext _context;
    public SysInfoRepository(AppDbContext context) => _context = context;

    public async Task<SysInfo?> GetByIdAsync(int sysInfoId)
        => await _context.SysInfos.FindAsync(sysInfoId);

    public async Task<List<SysInfo>> GetAllAsync()
        => await _context.SysInfos.ToListAsync();

    public async Task<List<SysInfo>> GetByUserIdAsync(int userId)
        => await _context.SysInfos.Where(s => s.UserId == userId).ToListAsync();

    public async Task AddAsync(SysInfo sysInfo)
        => await _context.SysInfos.AddAsync(sysInfo);

    public void Update(SysInfo sysInfo)
        => _context.SysInfos.Update(sysInfo);

    public void Delete(SysInfo sysInfo)
        => _context.SysInfos.Remove(sysInfo);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();
}
