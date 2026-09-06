using Backend.Models;

namespace Backend.Repositories;

public interface IOrderTimelineRepository
{
    Task<List<OrderTimeline>> GetByPurchaseIdAsync(long purchaseId);
    Task AddAsync(OrderTimeline timeline);
    Task SaveAsync();
}
