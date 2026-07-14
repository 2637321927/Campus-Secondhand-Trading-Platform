using Backend.Dtos.Home;
using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

public class HomeService : IHomeService
{
    private readonly IProductRepository _productRepo;
    private readonly ICategoryService _categoryService;
    private readonly ICollectionRepository _collectionRepo;
    private readonly IProductViewRepository _productViewRepo;
    //TODO：由于目前商品和收藏的模块还没完成，故只调用仓储层
    public HomeService(IProductRepository productRepo, ICategoryService categoryService, ICollectionRepository collectionRepo, IProductViewRepository productViewRepo)
    {
        _productRepo = productRepo;
        _categoryService = categoryService;
        _collectionRepo = collectionRepo;
        _productViewRepo = productViewRepo;
    }

    public async Task<HomeResponseDto> GetHomeDataAsync(int? userId)
    {
        //推荐商品：最新上架的10条
        //TODO：后续可以改成根据用户兴趣推荐
        var recommended = await GetRecommendedProductsAsync();
        var categories = await _categoryService.GetAllAsync();

        UserQuickEntryDto? quickEntry = null;
        //如果userId不为null，则查询用户的快捷入口数据
        //TODO：快捷入口数据需等待用户板块完成后再做调整
        if (userId != null)
        {
            var favorites = await _collectionRepo.GetByUserIdAsync(userId.Value);
            var published = await _productRepo.GetByUserIdAsync(userId.Value);

            quickEntry = new UserQuickEntryDto
            {
                FavoriteCount = favorites.Count,
                PublishedProductCount = published.Count,
                UnreadMessageCount = 0  // TODO：等消息模块做好后补上
            };
        }

        return new HomeResponseDto
        {
            RecommendedProducts = recommended,
            Categories = categories,
            UserQuickEntry = quickEntry
        };
    }

    public async Task<List<ProductCardDto>> GetRecommendedProductsAsync()
        => await GetLatestProductsAsync(10);

    public async Task<List<ProductCardDto>> GetHotProductsAsync()
        => await GetMostViewedProductsAsync(20);

    //以下为内部方法
    ///<summary>
    ///获取最新上架的count个在售商品（推荐）
    ///</summary>
    private async Task<List<ProductCardDto>> GetLatestProductsAsync(int count)
    {
        var products = await _productRepo.GetAllAsync();
        var filtered = products
            .Where(p => p.Status == ProductStatus.Available)
            .OrderByDescending(p => p.ReleaseDate)
            .Take(count)
            .ToList();

        var ids = filtered.Select(p => p.ProductId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(ids);

        return filtered.Select(p => ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0))).ToList();
    }

    ///<summary>
    ///获取浏览量最高的count个在售商品（热门）
    ///</summary>
    private async Task<List<ProductCardDto>> GetMostViewedProductsAsync(int count)
    {
        var products = await _productRepo.GetAllAsync();
        var available = products.Where(p => p.Status == ProductStatus.Available).ToList();

        var ids = available.Select(p => p.ProductId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(ids);

        return available
            .OrderByDescending(p => viewCounts.GetValueOrDefault(p.ProductId, 0))
            .ThenByDescending(p => p.ReleaseDate)
            .Take(count)
            .Select(p => ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();
    }

    private static ProductCardDto ToProductCard(Product p, int viewCount = 0) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        Price = p.Price,
        CoverImageUrl = p.Images?
            .OrderBy(i => i.ImgIndex)
            .FirstOrDefault()?.ImgFileId is long fileId
                ? $"/api/files/{fileId}" : null,
        SellerName = p.Seller?.UserName ?? "",
        ReleaseDate = p.ReleaseDate,
        ViewCount = viewCount
    };
}