using Backend.Dtos.Product;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ProductViewService : IProductViewService
{
    private readonly IProductViewRepository _viewRepo;

    public ProductViewService(IProductViewRepository viewRepo)
    {
        _viewRepo = viewRepo;
    }

    public async Task<List<BrowseHistoryDto>> GetBrowseHistoryAsync(int userId)
    {
        var views = await _viewRepo.GetByUserIdAsync(userId);
        return views.Select(ToDto).ToList();
    }

    public async Task ClearBrowseHistoryAsync(int userId)
    {
        await _viewRepo.DeleteByUserIdAsync(userId);
        await _viewRepo.SaveAsync();
    }

    public async Task DeleteBrowseHistoryItemAsync(int userId, long productId)
    {
        await _viewRepo.DeleteByUserIdAndProductIdAsync(userId, productId);
        await _viewRepo.SaveAsync();
    }

    private static BrowseHistoryDto ToDto(ProductView v)
    {
        var firstImageFileId = v.Product?.Images?
            .OrderBy(i => i.ImgIndex)
            .Select(i => (long?)i.ImgFileId)
            .FirstOrDefault();

        return new BrowseHistoryDto
        {
            ViewId = v.ViewId,
            ProductId = v.ProductId,
            ProductName = v.Product?.Name ?? "",
            ProductPrice = v.Product?.Price ?? 0,
            ProductImageFileId = firstImageFileId,
            ViewTime = v.ViewTime
        };
    }
}
