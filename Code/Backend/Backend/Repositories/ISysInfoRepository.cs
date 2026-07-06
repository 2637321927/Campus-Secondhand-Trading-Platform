using Backend.Models;

namespace Backend.Repositories;

public interface ISysInfoRepository
{
    Task<SysInfo?> GetByIdAsync(int sysInfoId);
    Task<List<SysInfo>> GetAllAsync();
    Task<List<SysInfo>> GetByUserIdAsync(int userId);
    Task AddAsync(SysInfo sysInfo);
    void Update(SysInfo sysInfo);
    void Delete(SysInfo sysInfo);
    Task SaveAsync();
}
