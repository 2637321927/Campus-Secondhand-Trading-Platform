using Backend.Models;

namespace Backend.Repositories;

public interface IRefundRepository
{
    Task<Refund?> GetByIdAsync(long refundId);
    Task<List<Refund>> GetAllAsync();
    Task<Refund?> GetByPurchaseIdAsync(long purchaseId);
    Task AddAsync(Refund refund);
    void Update(Refund refund);
    void Delete(Refund refund);
    Task SaveAsync();
}
