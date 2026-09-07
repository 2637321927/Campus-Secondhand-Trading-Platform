using Backend.Models;

namespace Backend.Repositories;

public interface IWorkOrderRepository
{
    IQueryable<WorkOrder> Query();
    Task<WorkOrder?> GetByIdAsync(long workOrderId);
    Task<List<WorkOrder>> GetAllAsync();
    Task<List<WorkOrder>> GetByInitiatorIdAsync(int userId);
    Task<List<WorkOrder>> GetByStatusAsync(string status);
    Task<List<WorkOrder>> GetRelatedByUserIdAsync(int userId, int type, string? status, bool includeAccused);
    Task<(List<WorkOrder> Items, int Total)> GetAdminPageAsync(
        int type,
        string? keyword,
        string? status,
        string? targetType,
        int page,
        int pageSize);
    Task<WorkOrder?> GetDetailAsync(long workOrderId);
    Task AddAsync(WorkOrder workOrder);
    void Update(WorkOrder workOrder);
    void Delete(WorkOrder workOrder);
    Task SaveAsync();
}
