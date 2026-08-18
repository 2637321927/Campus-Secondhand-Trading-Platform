using System.ComponentModel.DataAnnotations;
using Backend.Models.Enums;

namespace Backend.Dtos.Admin;

/// <summary>
/// 管理员商品列表项
/// </summary>
public class AdminProductListItemDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Info { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.PendingReview;
    public DateTime ReleaseDate { get; set; }
    public int UserId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }
    public int CommentCount { get; set; }
    public int ImageCount { get; set; }
    public string? RejectReason { get; set; }
    public int? ReviewedByAdminId { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

/// <summary>
/// 管理员商品分页结果
/// </summary>
public class AdminProductPageDto
{
    public List<AdminProductListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));
}

/// <summary>
/// 管理员商品详情
/// </summary>
public class AdminProductDetailDto : AdminProductListItemDto
{
    public List<AdminProductImageDto> Images { get; set; } = new();
    public List<AdminProductAuditLogDto> AuditLogs { get; set; } = new();
}

public class AdminProductImageDto
{
    public long FileId { get; set; }
    public int ImgIndex { get; set; }
}

/// <summary>
/// 商品审核日志
/// </summary>
public class AdminProductAuditLogDto
{
    public long AuditId { get; set; }
    public long ProductId { get; set; }
    public int AdminId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public ProductStatus OldStatus { get; set; }
    public ProductStatus NewStatus { get; set; }
    public DateTime CreateTime { get; set; }
}

/// <summary>
/// 驳回商品请求
/// </summary>
public class RejectProductDto
{
    [Required(ErrorMessage = "驳回原因不能为空")]
    [MaxLength(500, ErrorMessage = "驳回原因最多500个字符")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 强制下架请求
/// </summary>
public class RemoveProductDto
{
    [Required(ErrorMessage = "下架原因不能为空")]
    [MaxLength(500, ErrorMessage = "下架原因最多500个字符")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 商品审核统计
/// </summary>
public class AdminProductStatisticsDto
{
    public int TotalProducts { get; set; }
    public int AvailableCount { get; set; }
    public int SoldCount { get; set; }
    public int RemovedCount { get; set; }
    public int PendingReviewCount { get; set; }
    public int RejectedCount { get; set; }
    public int NewProductsToday { get; set; }
    public int TotalAuditLogs { get; set; }
    public int TodayAuditLogs { get; set; }
}
