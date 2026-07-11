using Backend.Dtos.Auth;
using Backend.Dtos.User;

namespace Backend.Services;

public interface IBaseUserService
{
    // 注册
    Task<UserDto> RegisterAsync(RegisterDto dto);
    // 登录
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    // 获取当前用户信息
    Task<MeResponseDto?> GetMeAsync(int userId);
    // 修改密码
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    // 发起重置密码
    Task RequestPasswordResetAsync(ResetPasswordRequestDto dto);
    // 确认重置密码
    Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto dto);
    // 权限检查
    Task<bool> CheckPermissionAsync(int userId, string requiredPermission);
    // 根据ID获取
    Task<UserDto?> GetByIdAsync(int userId);
}
