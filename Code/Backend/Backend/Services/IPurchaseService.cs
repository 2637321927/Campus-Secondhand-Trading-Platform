using Backend.Dtos.Product;

namespace Backend.Services;

public interface IPurchaseService
{
    Task<List<PurchaseDto>> GetMyPurchasesAsync(int buyerId);
    Task<List<PurchaseDto>> GetMySoldOrdersAsync(int sellerUserId);
    Task<List<PurchaseDto>> GetRelatedByUserIdAsync(int userId);
}
