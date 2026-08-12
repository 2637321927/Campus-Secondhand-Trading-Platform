using Backend.Dtos.Product;

namespace Backend.Services;

public interface IProductViewService
{
    Task<List<BrowseHistoryDto>> GetBrowseHistoryAsync(int userId);
    Task ClearBrowseHistoryAsync(int userId);
    Task DeleteBrowseHistoryItemAsync(int userId, long productId);
}
