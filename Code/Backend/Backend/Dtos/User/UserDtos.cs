namespace Backend.Dtos.User;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class UserDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime RegisterTime { get; set; }
}

/// <summary>
/// 个人中心资料 DTO
/// </summary>
public class UserProfileDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Gender { get; set; } = string.Empty;
    public long? AvatarFileId { get; set; }
    public DateTime RegisterTime { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Credit { get; set; }
    public string? Profile { get; set; }
    public DashboardCountsDto DashboardCounts { get; set; } = new();
}

/// <summary>
/// 个人中心统计数据
/// </summary>
public class DashboardCountsDto
{
    public int PublishedCount { get; set; }
    public int SoldCount { get; set; }
    public int PurchaseCount { get; set; }
    public int FavoriteCount { get; set; }
}

/// <summary>
/// 修改用户资料请求 DTO
/// 所有字段可选，只传需要修改的字段
/// </summary>
public class UpdateProfileDto
{
    /// <summary>
    /// 手机号（11位，唯一）
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 性别（male/female/unknown）
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// 昵称（最多20字符）
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// 个人简介（最多20字符）
    /// </summary>
    public string? Profile { get; set; }
}
