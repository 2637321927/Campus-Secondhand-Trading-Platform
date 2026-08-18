using Backend.Dtos.Product;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class PurchaseService : IPurchaseService
{
    private readonly IPurchaseRepository _purchaseRepo;

    public PurchaseService(IPurchaseRepository purchaseRepo)
    {
        _purchaseRepo = purchaseRepo;
    }

    public async Task<List<PurchaseDto>> GetMyPurchasesAsync(int buyerId)
    {
        var purchases = await _purchaseRepo.GetByBuyerIdAsync(buyerId);
        return purchases.Select(ToDto).ToList();
    }

    public async Task<List<PurchaseDto>> GetMySoldOrdersAsync(int sellerUserId)
    {
        var purchases = await _purchaseRepo.GetBySellerUserIdAsync(sellerUserId);
        return purchases.Select(ToDto).ToList();
    }

    public async Task<List<PurchaseDto>> GetRelatedByUserIdAsync(int userId)
    {
        var buyerOrders = await _purchaseRepo.GetByBuyerIdAsync(userId);
        var sellerOrders = await _purchaseRepo.GetBySellerUserIdAsync(userId);

        var merged = buyerOrders
            .Concat(sellerOrders)
            .GroupBy(p => p.PurchaseId)
            .Select(g => g.First())
            .OrderByDescending(p => p.CreateTime)
            .ToList();

        return merged.Select(ToDto).ToList();
    }

    private static PurchaseDto ToDto(Purchase p)
    {
        var firstImageFileId = p.Product?.Images?
            .OrderBy(i => i.ImgIndex)
            .Select(i => (long?)i.ImgFileId)
            .FirstOrDefault();

        return new PurchaseDto
        {
            PurchaseId = p.PurchaseId,
            Status = p.Status,
            CreateTime = p.CreateTime,
            CancelTime = p.CancelTime,
            PayTime = p.PayTime,
            ShippingTime = p.ShippingTime,
            DeliveryTime = p.DeliveryTime,
            CompleteTime = p.CompleteTime,
            ShippingFees = p.ShippingFees,
            ResponsibleForShip = p.ResponsibleForShip,
            BuyerId = p.BuyerId,
            BuyerName = p.Buyer?.UserName ?? "",
            ProductId = p.ProductId,
            ProductName = p.Product?.Name ?? "",
            ProductPrice = p.Product?.Price ?? 0,
            ProductImageFileId = firstImageFileId
        };
    }
}
