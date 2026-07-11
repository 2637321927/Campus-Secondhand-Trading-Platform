namespace Backend.Dtos.Auth;

///<summary>
/// 登录请求（邮箱和手机号二选一，优先用邮箱）
/// </summary>
public class LoginDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Password { get; set; } = string.Empty;
}

///<summary>
///登录响应（含JWT Token）
///</summary>
public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int UserType { get; set; }
    public string UserName { get; set; } = string.Empty;
}

///<summary>
///当前用户信息响应
///</summary>
public class MeResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserType { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int IsBanned { get; set; }
    public DateTime? BannedUntil { get; set; }
}

///<summary>
///修改密码请求
///</summary>
public class ChangePasswordDto
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

///<summary>
///重置密码请求（发起请求）邮箱和手机号二选一
///</summary>
public class ResetPasswordRequestDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}

///<summary>
///重置密码确认（确认重置）邮箱和手机号二选一
///</summary>
public class ResetPasswordConfirmDto
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string ResetToken { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

///<summary>
///权限检查请求
///</summary>
public class PermissionCheckDto
{
    public string RequiredPermission { get; set; } = string.Empty;
}
