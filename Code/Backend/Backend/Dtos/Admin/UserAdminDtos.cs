using System.ComponentModel.DataAnnotations;
using Backend.Dtos.Reputation;
using Backend.Models.Enums;

namespace Backend.Dtos.Admin;

/// <summary>
/// 管理员用户列表项
/// </summary>
public class AdminUserListItemDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int UserType { get; set; }
    public AccountStatus AccountStatus { get; set; } = AccountStatus.Normal;
    public int IsBanned { get; set; }
    public DateTime? BannedUntil { get; set; }
    public int Credit { get; set; }
    public DateTime RegisterTime { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
    public int WarningCount { get; set; }
    public int ViolationCount { get; set; }
}

/// <summary>
/// 管理员用户详情
/// </summary>
public class AdminUserDetailDto : AdminUserListItemDto
{
    public string Gender { get; set; } = "unknown";
    public string? Profile { get; set; }
    public long? AvatarFileId { get; set; }
}

/// <summary>
/// 管理员用户分页结果
/// </summary>
public class AdminUserPageDto
{
    public List<AdminUserListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}

/// <summary>
/// 管理员视角的举报/申诉列表项
/// </summary>
public class AdminWorkOrderDto
{
    public long WorkOrderId { get; set; }
    public int Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Info { get; set; }
    public string Status { get; set; } = "waiting";
    public DateTime CreateTime { get; set; }
    public string? Response { get; set; }
    public DateTime? ResponseTime { get; set; }
    public int InitiatorId { get; set; }
    public string InitiatorName { get; set; } = string.Empty;
    public int? AccusedId { get; set; }
    public string? AccusedName { get; set; }
    public long? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? AdminId { get; set; }
}

/// <summary>
/// 管理员用户信誉与违规概览
/// </summary>
public class AdminUserReputationDto
{
    public ReputationSummaryDto Summary { get; set; } = new();
    public int TotalViolations { get; set; }
    public int PendingViolations { get; set; }
    public int WarningCount { get; set; }
    public List<AdminUserWarningDto> RecentWarnings { get; set; } = new();
}

/// <summary>
/// 警告记录
/// </summary>
public class AdminUserWarningDto
{
    public long WarningId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public int AdminId { get; set; }
    public string AdminName { get; set; } = "管理员";
}

/// <summary>
/// 修改用户状态请求
/// </summary>
public class UpdateAdminUserStatusDto
{
    [Required]
    public AccountStatus Status { get; set; } = AccountStatus.Normal;

    public DateTime? BannedUntil { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

/// <summary>
/// 发送用户警告请求
/// </summary>
public class CreateUserWarningDto
{
    [Required(ErrorMessage = "警告内容不能为空")]
    [MaxLength(500, ErrorMessage = "警告内容最多500个字符")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 用户统计
/// </summary>
public class AdminUserStatisticsDto
{
    public int TotalUsers { get; set; }
    public int NormalUsers { get; set; }
    public int MutedUsers { get; set; }
    public int PublishRestrictedUsers { get; set; }
    public int BannedUsers { get; set; }
    public int NewUsersToday { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int UsersWithProducts { get; set; }
    public int TotalOrders { get; set; }
    public int TotalWorkOrders { get; set; }
    public int PendingWorkOrders { get; set; }
    public int TotalWarnings { get; set; }
}
