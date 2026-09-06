using Backend.Models;

namespace Backend.Repositories;

public interface IWorkOrderTimelineRepository
{
    IQueryable<WorkOrderTimeline> Query();
    Task<List<WorkOrderTimeline>> GetByWorkOrderIdAsync(long workOrderId);
    Task AddAsync(WorkOrderTimeline timeline);
    Task SaveAsync();
}
