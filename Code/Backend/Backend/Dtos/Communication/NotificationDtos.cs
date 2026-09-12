using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos.Communication;

/// <summary>
/// 通知公告模块 DTO — 站内通知（公告 / 用户警告）与管理端公告管理
/// </summary>

/// <summary>
/// 站内通知项（公告 / 用户警告）
/// </summary>
public class NotificationDto
{
    public long NotificationId { get; set; }

    /// <summary>通知类型：announcement=系统公告，warning=用户警告</summary>
    public string Type { get; set; } = "announcement";

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public bool IsRead { get; set; }

    /// <summary>
    /// 关联对象 ID（如商品 ID、订单 ID），无则 null
    /// </summary>
    public long? RelatedId { get; set; }
}

/// <summary>
/// 管理员发布公告请求
/// </summary>
public class CreateAnnouncementDto
{
    [Required(ErrorMessage = "公告标题不能为空")]
    [MaxLength(100, ErrorMessage = "公告标题最多100个字符")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "公告内容不能为空")]
    [MaxLength(2000, ErrorMessage = "公告内容最多2000个字符")]
    public string Content { get; set; } = string.Empty;

    public bool IsPinned { get; set; }

    /// <summary>
    /// draft=草稿，published=发布
    /// </summary>
    public string Status { get; set; } = "published";
}

/// <summary>
/// 公告响应
/// </summary>
public class AnnouncementDto
{
    public int AnnouncementId { get; set; }
    public int Id => AnnouncementId;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public string Status { get; set; } = "published";
    public DateTime ReleaseTime { get; set; }
    public DateTime? PublishTime { get; set; }
    public int AdminId { get; set; }
}

public class AnnouncementPageDto
{
    public List<AnnouncementDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}

public class UpdateAnnouncementDto
{
    [MaxLength(100)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Content { get; set; }

    public bool? IsPinned { get; set; }

    public string? Status { get; set; }
}

public class AnnouncementStatisticsDto
{
    public int Total { get; set; }
    public int Published { get; set; }
    public int Draft { get; set; }
    public int Archived { get; set; }
}
