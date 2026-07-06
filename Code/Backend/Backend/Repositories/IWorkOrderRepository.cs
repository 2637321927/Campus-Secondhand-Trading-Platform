using Backend.Models;

namespace Backend.Repositories;

public interface IWorkOrderRepository
{
    Task<WorkOrder?> GetByIdAsync(long workOrderId);
    Task<List<WorkOrder>> GetAllAsync();
    Task<List<WorkOrder>> GetByInitiatorIdAsync(int userId);
    Task<List<WorkOrder>> GetByStatusAsync(string status);
    Task AddAsync(WorkOrder workOrder);
    void Update(WorkOrder workOrder);
    void Delete(WorkOrder workOrder);
    Task SaveAsync();
}
