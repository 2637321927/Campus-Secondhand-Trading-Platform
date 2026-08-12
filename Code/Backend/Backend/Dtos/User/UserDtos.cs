namespace Backend.Dtos.User;

public class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime RegisterTime { get; set; }
}

/// <summary>
/// 个人中心资料响应（扁平化结构，便于前端直接取用）
/// </summary>
public class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Gender { get; set; } = "unknown";
    public string? Profile { get; set; }
    public long? AvatarFileId { get; set; }
    public int Credit { get; set; }
    public DateTime RegisterTime { get; set; }
}

/// <summary>
/// 修改个人中心资料请求（支持部分更新，仅传入需要修改的字段即可）
/// 注意：邮箱不可修改，故此处不提供 Email 字段
/// </summary>
public class UpdateProfileDto
{
    public string? UserName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? Profile { get; set; }
}

/// <summary>
/// 头像上传成功响应
/// </summary>
public class AvatarUploadResponseDto
{
    public long AvatarFileId { get; set; }
    public string FileName { get; set; } = string.Empty;
}
