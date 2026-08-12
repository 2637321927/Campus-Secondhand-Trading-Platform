using Backend.Models;

namespace Backend.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(long paymentId);
    Task<List<Payment>> GetByPurchaseIdAsync(long purchaseId);
    Task<Payment?> GetPendingByPurchaseIdAsync(long purchaseId);
    Task AddAsync(Payment payment);
    void Update(Payment payment);
    Task SaveAsync();
}
