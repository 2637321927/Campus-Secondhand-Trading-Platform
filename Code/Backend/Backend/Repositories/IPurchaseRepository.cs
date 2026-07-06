using Backend.Models;

namespace Backend.Repositories;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByIdAsync(long purchaseId);
    Task<List<Purchase>> GetAllAsync();
    Task<List<Purchase>> GetByBuyerIdAsync(int buyerId);
    Task<List<Purchase>> GetByStatusAsync(string status);
    Task AddAsync(Purchase purchase);
    void Update(Purchase purchase);
    void Delete(Purchase purchase);
    Task SaveAsync();
}
