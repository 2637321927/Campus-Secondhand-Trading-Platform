using Backend.Dtos.Admin;

namespace Backend.Services;

public interface IAdminOrderService
{
    Task<AdminOrderPageDto> GetOrdersAsync(
        long? orderId,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize);

    Task<AdminOrderStatisticsDto> GetStatisticsAsync();
    Task<AdminOrderDto?> GetOrderAsync(long orderId);
    Task<AdminOrderDto?> CancelOrderAsync(long orderId, int adminId);
    Task<AdminOrderDto?> CompleteOrderAsync(long orderId, int adminId);
}
