using Backend.Dtos.Auth;
using Backend.Dtos.User;

namespace Backend.Services;

public interface IBaseUserService
{
    ///<summary>
    ///注册
    ///</summary>
    Task<UserDto> RegisterAsync(RegisterDto dto);
    ///<summary>
    ///登录
    ///</summary>
    Task<LoginResponseDto> LoginAsync(LoginDto dto);
    ///<summary>
    ///获取当前用户信息
    ///</summary>
    Task<MeResponseDto?> GetMeAsync(int userId);
    ///<summary>
    ///修改密码
    ///</summary>
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    ///<summary>
    ///发起重置密码
    ///</summary>
    Task RequestPasswordResetAsync(ResetPasswordRequestDto dto);
    ///<summary>
    ///确认重置密码
    ///</summary>
    Task ConfirmPasswordResetAsync(ResetPasswordConfirmDto dto);
    ///<summary>
    ///权限检查
    ///</summary>
    Task<bool> CheckPermissionAsync(int userId, string requiredPermission);
    ///<summary>
    ///根据ID获取
    ///</summary>
    Task<UserDto?> GetByIdAsync(int userId);
}
