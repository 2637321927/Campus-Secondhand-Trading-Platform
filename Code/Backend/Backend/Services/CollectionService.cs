using Backend.Dtos.Product;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class CollectionService : ICollectionService
{
    private readonly ICollectionRepository _collectionRepo;
    private readonly IProductRepository _productRepo;
    private readonly IProductViewRepository _productViewRepo;

    public CollectionService(ICollectionRepository collectionRepo,IProductRepository productRepo, IProductViewRepository productViewRepo)
    {
        _collectionRepo = collectionRepo;
        _productRepo = productRepo;
        _productViewRepo = productViewRepo;
    }

    public async Task<bool> ToggleAsync(long productId, int userId)
    {
        //检查商品是否存在
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null)
            throw new ArgumentException("商品不存在");
        //禁止收藏自己的商品
        if (product.UserId == userId)
            throw new InvalidOperationException("不能收藏自己发布的商品");
        //检查是否已收藏
        var existing = await _collectionRepo.GetByIdAsync(productId, userId);
        if (existing != null){
            //已收藏就取消
            _collectionRepo.Delete(existing);
            await _collectionRepo.SaveAsync();
            return false;
        }
        else {
            //未收藏就添加
            await _collectionRepo.AddAsync(new Collection
            {
                ProductId = productId,
                UserId = userId,
                CollectionTime = DateTime.Now
            });
            await _collectionRepo.SaveAsync();
            return true;
        }
    }

    public async Task<bool> IsCollectedAsync(long productId, int userId)
        => await _collectionRepo.IsCollectedAsync(productId, userId);

    public async Task<List<ProductCardDto>> GetMyCollectionsAsync(int userId)
    {
        var collections = await _collectionRepo.GetByUserIdAsync(userId);

        //按收藏时间倒序，取商品列表
        var products = collections
            .OrderByDescending(c => c.CollectionTime)
            .Where(c => c.Product != null)
            .Select(c => c.Product!)
            .ToList();
        //批量查浏览量
        var ids = products.Select(p => p.ProductId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(ids);

        //映射为ProductCardDto
        return products.Select(p => ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0))).ToList();
    }

    public async Task<int> GetCollectionCountAsync(int userId)
    {
        var collections = await _collectionRepo.GetByUserIdAsync(userId);
        return collections.Count;
    }

    public async Task<List<ProductCardDto>> SearchAsync(int userId, string keyword)
    {
        var collections = await _collectionRepo.GetByUserIdAsync(userId);

        var products = collections
            .OrderByDescending(c => c.CollectionTime)//按收藏时间倒序
            .Where(c => c.Product != null && c.Product!.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))//包含关键词且忽略大小写
            .Select(c => c.Product!)
            .ToList();
        var ids = products.Select(p => p.ProductId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(ids);
        return products.Select(p => ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0))).ToList();
    }

    public async Task<int> BatchDeleteAsync(int userId, List<long> productIds)
    {
        int deleted = 0;
        foreach (var productId in productIds)
        {
            var collection = await _collectionRepo.GetByIdAsync(productId, userId);
            if (collection != null)
            {
                _collectionRepo.Delete(collection);
                deleted++;
            }
        }
        if (deleted > 0)
            await _collectionRepo.SaveAsync();
        return deleted;
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
