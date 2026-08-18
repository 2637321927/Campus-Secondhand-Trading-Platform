using Backend.Dtos.Admin;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class AdminProductManagementService : IAdminProductManagementService
{
    private readonly IProductRepository _productRepo;
    private readonly IProductAuditLogRepository _auditRepo;
    private readonly IProductViewRepository _viewRepo;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IProductCommentRepository _commentRepo;
    private readonly IProdImageService _prodImageService;

    public AdminProductManagementService(
        IProductRepository productRepo,
        IProductAuditLogRepository auditRepo,
        IProductViewRepository viewRepo,
        ICollectionRepository collectionRepo,
        IProductCommentRepository commentRepo,
        IProdImageService prodImageService)
    {
        _productRepo = productRepo;
        _auditRepo = auditRepo;
        _viewRepo = viewRepo;
        _collectionRepo = collectionRepo;
        _commentRepo = commentRepo;
        _prodImageService = prodImageService;
    }

    public async Task<AdminProductPageDto> GetProductsAsync(
        string? keyword,
        int? status,
        long? categoryId,
        int? sellerId,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var (items, total) = await _productRepo.GetAdminPageAsync(
            keyword, status, categoryId, sellerId, page, pageSize);

        var ids = items.Select(p => p.ProductId).ToList();
        var viewCounts = await _viewRepo.GetViewCountsAsync(ids);
        var favoriteCounts = await _collectionRepo.GetCountsByProductIdsAsync(ids);
        var commentCounts = await _commentRepo.GetCountsByProductIdsAsync(ids);

        var result = items.Select(p => ToListItem(
            p,
            viewCounts.GetValueOrDefault(p.ProductId, 0),
            favoriteCounts.GetValueOrDefault(p.ProductId, 0),
            commentCounts.GetValueOrDefault(p.ProductId, 0))).ToList();

        return new AdminProductPageDto
        {
            Items = result,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<AdminProductDetailDto?> GetProductDetailAsync(long productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        return await ToDetailAsync(product);
    }

    public async Task<AdminProductPageDto> GetPendingReviewAsync(int page, int pageSize)
        => await GetProductsAsync(null, (int)ProductStatus.PendingReview, null, null, page, pageSize);

    public async Task<AdminProductDetailDto?> ApproveAsync(long productId, int adminId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.Status != ProductStatus.PendingReview)
            throw new InvalidOperationException("只有待审核商品可以审核通过");

        var oldStatus = product.Status;
        product.Status = ProductStatus.Available;
        product.RejectReason = null;
        product.ReviewedByAdminId = adminId;
        product.ReviewedAt = DateTime.Now;

        await SaveWithAuditAsync(product, "approve", oldStatus, null, adminId);
        return await ToDetailAsync(product);
    }

    public async Task<AdminProductDetailDto?> RejectAsync(long productId, RejectProductDto dto, int adminId)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("驳回原因不能为空");

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.Status != ProductStatus.PendingReview)
            throw new InvalidOperationException("只有待审核商品可以驳回");

        var oldStatus = product.Status;
        product.Status = ProductStatus.Rejected;
        product.RejectReason = dto.Reason.Trim();
        product.ReviewedByAdminId = adminId;
        product.ReviewedAt = DateTime.Now;

        await SaveWithAuditAsync(product, "reject", oldStatus, dto.Reason.Trim(), adminId);
        return await ToDetailAsync(product);
    }

    public async Task<AdminProductDetailDto?> RemoveAsync(long productId, RemoveProductDto dto, int adminId)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
            throw new ArgumentException("下架原因不能为空");

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.Status == ProductStatus.Sold)
            throw new InvalidOperationException("已售商品不能强制下架");

        var oldStatus = product.Status;
        product.Status = ProductStatus.Removed;

        await SaveWithAuditAsync(product, "remove", oldStatus, dto.Reason.Trim(), adminId);
        return await ToDetailAsync(product);
    }

    public async Task<AdminProductDetailDto?> RestoreAsync(long productId, int adminId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.Status != ProductStatus.Removed)
            throw new InvalidOperationException("只有已下架商品可以恢复");

        var oldStatus = product.Status;
        product.Status = ProductStatus.Available;

        await SaveWithAuditAsync(product, "restore", oldStatus, null, adminId);
        return await ToDetailAsync(product);
    }

    public async Task<bool> DeleteAsync(long productId, int adminId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return false;

        var imageIds = product.Images.Select(i => i.ImgFileId).ToList();
        if (imageIds.Count > 0)
            await _prodImageService.DeleteProductImagesAsync(imageIds);

        product.Images.Clear();
        _productRepo.Delete(product);

        try
        {
            await _productRepo.SaveAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("商品存在关联订单或会话，无法删除，建议使用强制下架");
        }
    }

    public async Task<List<AdminProductAuditLogDto>?> GetAuditLogsAsync(long productId)
    {
        if (await _productRepo.GetByIdAsync(productId) == null) return null;

        var logs = await _auditRepo.GetByProductIdAsync(productId);
        return logs.Select(l => new AdminProductAuditLogDto
        {
            AuditId = l.AuditId,
            ProductId = l.ProductId,
            AdminId = l.AdminId,
            Action = l.Action,
            Reason = l.Reason,
            OldStatus = l.OldStatus,
            NewStatus = l.NewStatus,
            CreateTime = l.CreateTime
        }).ToList();
    }

    public async Task<AdminProductStatisticsDto> GetStatisticsAsync()
    {
        var today = DateTime.Today;

        return new AdminProductStatisticsDto
        {
            TotalProducts = await _productRepo.Query().CountAsync(),
            AvailableCount = await _productRepo.Query().CountAsync(p => p.Status == ProductStatus.Available),
            SoldCount = await _productRepo.Query().CountAsync(p => p.Status == ProductStatus.Sold),
            RemovedCount = await _productRepo.Query().CountAsync(p => p.Status == ProductStatus.Removed),
            PendingReviewCount = await _productRepo.Query().CountAsync(p => p.Status == ProductStatus.PendingReview),
            RejectedCount = await _productRepo.Query().CountAsync(p => p.Status == ProductStatus.Rejected),
            NewProductsToday = await _productRepo.Query().CountAsync(p => p.ReleaseDate >= today),
            TotalAuditLogs = await _auditRepo.Query().CountAsync(),
            TodayAuditLogs = await _auditRepo.Query().CountAsync(l => l.CreateTime >= today)
        };
    }

    private async Task SaveWithAuditAsync(
        Product product,
        string action,
        ProductStatus oldStatus,
        string? reason,
        int adminId)
    {
        _productRepo.Update(product);
        await _auditRepo.AddAsync(new ProductAuditLog
        {
            ProductId = product.ProductId,
            AdminId = adminId,
            Action = action,
            Reason = reason,
            OldStatus = oldStatus,
            NewStatus = product.Status,
            CreateTime = DateTime.Now
        });
        await _productRepo.SaveAsync();
    }

    private async Task<AdminProductDetailDto> ToDetailAsync(Product product)
    {
        var viewCount = await _viewRepo.GetViewCountAsync(product.ProductId);
        var favoriteCounts = await _collectionRepo.GetCountsByProductIdsAsync(new[] { product.ProductId });
        var commentCounts = await _commentRepo.GetCountsByProductIdsAsync(new[] { product.ProductId });
        var logs = await _auditRepo.GetByProductIdAsync(product.ProductId);

        var listItem = ToListItem(
            product,
            viewCount,
            favoriteCounts.GetValueOrDefault(product.ProductId, 0),
            commentCounts.GetValueOrDefault(product.ProductId, 0));

        return new AdminProductDetailDto
        {
            ProductId = listItem.ProductId,
            Name = listItem.Name,
            Price = listItem.Price,
            Info = listItem.Info,
            Status = listItem.Status,
            ReleaseDate = listItem.ReleaseDate,
            UserId = listItem.UserId,
            SellerName = listItem.SellerName,
            CategoryId = listItem.CategoryId,
            CategoryName = listItem.CategoryName,
            ViewCount = listItem.ViewCount,
            FavoriteCount = listItem.FavoriteCount,
            CommentCount = listItem.CommentCount,
            ImageCount = listItem.ImageCount,
            RejectReason = listItem.RejectReason,
            ReviewedByAdminId = listItem.ReviewedByAdminId,
            ReviewedAt = listItem.ReviewedAt,
            Images = product.Images
                .OrderBy(i => i.ImgIndex)
                .Select(i => new AdminProductImageDto
                {
                    FileId = i.ImgFileId,
                    ImgIndex = i.ImgIndex
                }).ToList(),
            AuditLogs = logs.Select(l => new AdminProductAuditLogDto
            {
                AuditId = l.AuditId,
                ProductId = l.ProductId,
                AdminId = l.AdminId,
                Action = l.Action,
                Reason = l.Reason,
                OldStatus = l.OldStatus,
                NewStatus = l.NewStatus,
                CreateTime = l.CreateTime
            }).ToList()
        };
    }

    private static AdminProductListItemDto ToListItem(
        Product product,
        int viewCount,
        int favoriteCount,
        int commentCount) => new()
    {
        ProductId = product.ProductId,
        Name = product.Name,
        Price = product.Price,
        Info = product.Info,
        Status = product.Status,
        ReleaseDate = product.ReleaseDate,
        UserId = product.UserId,
        SellerName = product.Seller?.UserName ?? "",
        CategoryId = product.CategoryId,
        CategoryName = product.Category?.CategoryName,
        ViewCount = viewCount,
        FavoriteCount = favoriteCount,
        CommentCount = commentCount,
        ImageCount = product.Images?.Count ?? 0,
        RejectReason = product.RejectReason,
        ReviewedByAdminId = product.ReviewedByAdminId,
        ReviewedAt = product.ReviewedAt
    };
}
