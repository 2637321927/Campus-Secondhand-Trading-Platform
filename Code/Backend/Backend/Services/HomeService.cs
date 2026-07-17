using Backend.Dtos.Home;
using Backend.Dtos.Product;
using Backend.Repositories;

namespace Backend.Services;

public class HomeService : IHomeService
{
    private readonly IProductService _productService;
    private readonly IProductRepository _productRepo;
    private readonly ICategoryService _categoryService;
    private readonly ICollectionService _collectionService;
    private readonly IProductViewRepository _productViewRepo;

    public HomeService(
        IProductService productService,
        IProductRepository productRepo,
        ICategoryService categoryService,
        ICollectionService collectionService,
        IProductViewRepository productViewRepo)
    {
        _productService = productService;
        _productRepo = productRepo;
        _categoryService = categoryService;
        _collectionService = collectionService;
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
            var favoriteCount = await _collectionService.GetCollectionCountAsync(userId.Value);
            var published = await _productRepo.GetByUserIdAsync(userId.Value);

            quickEntry = new UserQuickEntryDto
            {
                FavoriteCount = favoriteCount,
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
        => await _productService.QueryProductCardsAsync(null, ProductService.Latest, 10);

    public async Task<List<ProductCardDto>> GetHotProductsAsync()
        => await _productService.QueryProductCardsAsync(null, ProductService.Hottest, 20);

}