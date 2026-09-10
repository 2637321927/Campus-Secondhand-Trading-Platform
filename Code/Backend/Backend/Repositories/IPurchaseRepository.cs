using Backend.Models;

namespace Backend.Repositories;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByIdAsync(long purchaseId);
    Task<List<Purchase>> GetAllAsync();
    Task<List<Purchase>> GetByBuyerIdAsync(int buyerId);
    Task<List<Purchase>> GetBySellerUserIdAsync(int sellerUserId);
    Task<List<Purchase>> GetBySellerIdAsync(int sellerId);
    Task<List<Purchase>> GetByProductIdAsync(long productId);
    Task<List<Purchase>> GetByStatusAsync(string status);
    Task<(List<Purchase> Items, int Total)> GetAdminPageAsync(
        long? orderId,
        string? status,
        DateTime? startDate,
        DateTime? endDate,
        int page,
        int pageSize);
    IQueryable<Purchase> Query();
    Task AddAsync(Purchase purchase);
    void Update(Purchase purchase);
    void Delete(Purchase purchase);
    Task SaveAsync();
}
