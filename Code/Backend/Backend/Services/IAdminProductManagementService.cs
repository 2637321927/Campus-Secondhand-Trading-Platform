using Backend.Dtos.Admin;

namespace Backend.Services;

public interface IAdminProductManagementService
{
    Task<AdminProductPageDto> GetProductsAsync(
        string? keyword,
        int? status,
        long? categoryId,
        int? sellerId,
        int page,
        int pageSize);

    Task<AdminProductDetailDto?> GetProductDetailAsync(long productId);
    Task<AdminProductPageDto> GetPendingReviewAsync(int page, int pageSize);
    Task<AdminProductDetailDto?> ApproveAsync(long productId, int adminId);
    Task<AdminProductDetailDto?> RejectAsync(long productId, RejectProductDto dto, int adminId);
    Task<AdminProductDetailDto?> RemoveAsync(long productId, RemoveProductDto dto, int adminId);
    Task<AdminProductDetailDto?> RestoreAsync(long productId, int adminId);
    Task<bool> DeleteAsync(long productId, int adminId);
    Task<List<AdminProductAuditLogDto>?> GetAuditLogsAsync(long productId);
    Task<AdminProductStatisticsDto> GetStatisticsAsync();
}
