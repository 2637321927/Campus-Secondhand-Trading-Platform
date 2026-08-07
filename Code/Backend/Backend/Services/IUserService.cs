using Backend.Dtos.User;
using Microsoft.AspNetCore.Http;

namespace Backend.Services;

/// <summary>
/// 用户中心服务接口（个人中心资料管理）
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取当前用户的个人中心资料
    /// </summary>
    Task<UserProfileDto?> GetProfileAsync(int userId);

    /// <summary>
    /// 修改当前用户的个人资料
    /// </summary>
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);

    /// <summary>
    /// 上传或更换当前用户头像
    /// </summary>
    Task<UserProfileDto> UpdateAvatarAsync(int userId, IFormFile file);
}
