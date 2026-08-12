using Backend.Models;

namespace Backend.Repositories;

public interface IBaseUserRepository
{
    Task<BaseUser?> GetByIdAsync(int userId);
    Task<List<BaseUser>> GetAllAsync();
    Task<BaseUser?> GetByEmailAsync(string email);
    Task<BaseUser?> GetByPhoneAsync(string phone);
    Task AddAsync(BaseUser user);
    void Update(BaseUser user);
    void Delete(BaseUser user);
    Task SaveAsync();

    /// <summary>
    /// 将所有以指定文件作为头像的用户的头像重置为默认头像文件ID。
    /// 用于删除文件前清理头像引用，避免外键悬空。
    /// </summary>
    Task ResetAvatarByFileIdAsync(long fileId, long defaultFileId);
}
