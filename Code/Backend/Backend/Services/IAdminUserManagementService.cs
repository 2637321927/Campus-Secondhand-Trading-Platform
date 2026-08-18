using Backend.Dtos.Admin;
using Backend.Dtos.Product;

namespace Backend.Services;

public interface IAdminUserManagementService
{
    Task<AdminUserPageDto> GetUsersAsync(
        string? keyword,
        int? userType,
        int? accountStatus,
        int? creditMin,
        int? creditMax,
        DateTime? registerStart,
        DateTime? registerEnd,
        int page,
        int pageSize);

    Task<AdminUserDetailDto?> GetUserDetailAsync(int userId);
    Task<List<ProductDto>?> GetUserProductsAsync(int userId);
    Task<List<PurchaseDto>?> GetUserOrdersAsync(int userId);
    Task<List<AdminWorkOrderDto>?> GetUserReportsAsync(int userId);
    Task<List<AdminWorkOrderDto>?> GetUserAppealsAsync(int userId);
    Task<AdminUserReputationDto> GetUserReputationAsync(int userId);
    Task<AdminUserDetailDto?> UpdateUserStatusAsync(int userId, UpdateAdminUserStatusDto dto);
    Task<AdminUserWarningDto?> CreateWarningAsync(int userId, CreateUserWarningDto dto, int adminId);
    Task<AdminUserStatisticsDto> GetStatisticsAsync();
}
