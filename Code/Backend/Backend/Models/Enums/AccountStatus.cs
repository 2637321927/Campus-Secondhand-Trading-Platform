namespace Backend.Models.Enums;

/// <summary>
/// 账号状态：0=正常，1=禁言，2=限制发布，3=封禁
/// </summary>
public enum AccountStatus
{
    Normal = 0,
    Muted = 1,
    PublishRestricted = 2,
    Banned = 3
}
