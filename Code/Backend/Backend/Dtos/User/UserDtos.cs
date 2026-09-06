using System.ComponentModel.DataAnnotations;

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

/// <summary>
/// 地址响应 DTO
/// </summary>
public class AddressDto
{
    public int AddressId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string DetailAddress { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

/// <summary>
/// 新增地址请求 DTO
/// </summary>
public class CreateAddressDto
{
    [Required(ErrorMessage = "收货人姓名不能为空")]
    [MaxLength(10, ErrorMessage = "收货人姓名最多10个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "手机号不能为空")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "手机号必须是11位")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "详细地址不能为空")]
    [MaxLength(50, ErrorMessage = "详细地址最多50个字符")]
    public string DetailAddress { get; set; } = string.Empty;

    /// <summary>
    /// 是否设为默认地址：true=是，false=否
    /// </summary>
    public bool IsDefault { get; set; }
}

/// <summary>
/// 修改地址请求 DTO（支持部分更新，仅传入需要修改的字段即可）
/// </summary>
public class UpdateAddressDto
{
    [MaxLength(10, ErrorMessage = "收货人姓名最多10个字符")]
    public string? Name { get; set; }

    [StringLength(11, MinimumLength = 11, ErrorMessage = "手机号必须是11位")]
    public string? PhoneNumber { get; set; }

    [MaxLength(50, ErrorMessage = "详细地址最多50个字符")]
    public string? DetailAddress { get; set; }
}
