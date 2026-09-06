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

    public async Task<BaseUser?> GetByIdWithProfileAsync(int userId)
        => await _context.BaseUsers
            .Include(u => u.NormUser)
            .Include(u => u.AdminUser)
            .FirstOrDefaultAsync(u => u.UserId == userId);

    public async Task<List<BaseUser>> GetAllAsync()
        => await _context.BaseUsers.ToListAsync();

    public async Task<BaseUser?> GetByEmailAsync(string email)
        => await _context.BaseUsers.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<BaseUser?> GetByPhoneAsync(string phone)
        => await _context.BaseUsers.FirstOrDefaultAsync(u => u.PhoneNumber == phone);

    public async Task<(List<BaseUser> Items, int Total)> GetAdminPageAsync(
        string? keyword,
        int? userType,
        int? accountStatus,
        int? creditMin,
        int? creditMax,
        DateTime? registerStart,
        DateTime? registerEnd,
        int page,
        int pageSize)
    {
        var query = _context.BaseUsers
            .Include(u => u.NormUser)
            .AsQueryable();

        query = ApplyAdminFilters(query, keyword, userType, accountStatus, creditMin, creditMax, registerStart, registerEnd);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.RegisterTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> CountUsersAsync(
        int? userType,
        int? accountStatus,
        DateTime? registerStart,
        DateTime? registerEnd)
    {
        var query = _context.BaseUsers.AsQueryable();
        query = ApplyAdminFilters(query, null, userType, accountStatus, null, null, registerStart, registerEnd);
        return await query.CountAsync();
    }

    private static IQueryable<BaseUser> ApplyAdminFilters(
        IQueryable<BaseUser> query,
        string? keyword,
        int? userType,
        int? accountStatus,
        int? creditMin,
        int? creditMax,
        DateTime? registerStart,
        DateTime? registerEnd)
    {
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            query = query.Where(u =>
                u.Email.Contains(kw) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(kw)) ||
                (u.NormUser != null && u.NormUser.UserName.Contains(kw)));
        }

        if (userType.HasValue)
            query = query.Where(u => u.UserType == userType.Value);

        if (accountStatus.HasValue)
            query = query.Where(u => u.AccountStatus == (Backend.Models.Enums.AccountStatus)accountStatus.Value);

        if (creditMin.HasValue)
            query = query.Where(u => u.NormUser != null && u.NormUser.Credit >= creditMin.Value);

        if (creditMax.HasValue)
            query = query.Where(u => u.NormUser != null && u.NormUser.Credit <= creditMax.Value);

        if (registerStart.HasValue)
            query = query.Where(u => u.RegisterTime >= registerStart.Value);

        if (registerEnd.HasValue)
            query = query.Where(u => u.RegisterTime <= registerEnd.Value);

        return query;
    }

    public async Task AddAsync(BaseUser user)
        => await _context.BaseUsers.AddAsync(user);

    public void Update(BaseUser user)
        => _context.BaseUsers.Update(user);

    public void Delete(BaseUser user)
        => _context.BaseUsers.Remove(user);

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();

    public async Task ResetAvatarByFileIdAsync(long fileId, long defaultFileId)
    {
        var users = await _context.BaseUsers
            .Where(u => u.AvatarFileId == fileId)
            .ToListAsync();

        foreach (var u in users)
            u.AvatarFileId = defaultFileId;
    }
}
