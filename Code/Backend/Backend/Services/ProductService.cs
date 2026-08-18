using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

public class ProductService : IProductService
{
    
    private readonly IProductRepository _productRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IProductViewRepository _productViewRepo;
    private readonly IProdImageService _prodImage;
    private readonly ISearchService _searchService;
    private readonly IBaseUserRepository _baseUserRepo;

    public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo, IProductViewRepository productViewRepo, IProdImageService prodImageService, ISearchService searchService, IBaseUserRepository baseUserRepo)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _productViewRepo = productViewRepo;
        _prodImage = prodImageService;
        _searchService = searchService;
        _baseUserRepo = baseUserRepo;
    }

    public async Task<ProductDto?> GetByIdAsync(long productId, int userId)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;
        if (userId != -1 && product.UserId != userId) await RecordViewAsync(productId, userId);
        var viewCount = await _productViewRepo.GetViewCountAsync(productId);
        return ToDto(product, viewCount);

    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _productRepo.GetAllAsync();
        var viewCounts = await _productViewRepo.GetViewCountsAsync(
            products.Select(product => product.ProductId));

        return products
            .OrderByDescending(product => product.ReleaseDate)
            .Select(product => ToDto(
                product,
                viewCounts.GetValueOrDefault(product.ProductId, 0)))
            .ToList();
    }

    public async Task RecordViewAsync(long productId, int userId)
    {
        await _productViewRepo.AddAsync(new ProductView
        {
            ProductId = productId,
            UserId = userId,
            ViewTime = DateTime.Now
        });
        await _productViewRepo.SaveAsync();
    }

    public async Task<List<long>> GetProductIdsByUserIdAsync(int userId)
    {
        var products = await _productRepo.GetByUserIdAsync(userId);

        return products
            .OrderByDescending(product => product.ReleaseDate)
            .Select(product => product.ProductId)
            .ToList();
    }

    public async Task<List<ProductDto>> GetProductsByUserIdAsync(int userId)
    {
        var products = await _productRepo.GetByUserIdAsync(userId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(
            products.Select(p => p.ProductId));

        return products
            .OrderByDescending(p => p.ReleaseDate)
            .Select(p => ToDto(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();
    }

    public async Task<List<ProductDto>> GetSoldProductsByUserIdAsync(int userId)
    {
        var products = await _productRepo.GetSoldByUserIdAsync(userId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(
            products.Select(p => p.ProductId));

        return products
            .OrderByDescending(p => p.ReleaseDate)
            .Select(p => ToDto(p, viewCounts.GetValueOrDefault(p.ProductId, 0)))
            .ToList();
    }

    public async Task<ProductDto?> CreateAsync(int userId, CreateProductDto dto)
    {
        var user = await _baseUserRepo.GetByIdAsync(userId);
        if (user == null)
            throw new ArgumentException("用户不存在");

        if (user.AccountStatus == AccountStatus.Banned ||
            user.AccountStatus == AccountStatus.PublishRestricted)
            throw new UnauthorizedAccessException("当前账号状态不允许发布商品");

        if (await _categoryRepo.GetByIdAsync(dto.CategoryId) == null)
        {
            
            throw new ArgumentException("Category does not exist.");

        }

        var product = new Product
        {

            Name = dto.Name,
            Price = dto.Price,
            Info = dto.Info,
            Status = ProductStatus.PendingReview,
            UserId = userId,
            ReleaseDate = DateTime.Now,
            CategoryId = dto.CategoryId,
            ShippingType = dto.ShippingType,
            ShippingFee = dto.ShippingFee,
            AllowPickup = dto.AllowPickup

        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveAsync();

        _ = _searchService.NotifyProductCreatedAsync(product.ProductId);

        if (dto.Images != null && dto.Images.Count > 0)
        {
            await _prodImage.UploadProductImagesAsync(dto.Images, product.ProductId, userId);
        }

        return ToDto(product);
        
    }

    public async Task<ProductDto?> UpdateAsync(long productId, int userId, UpdateProductDto dto)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (product.UserId != userId)
        {

            throw new UnauthorizedAccessException("You do not have permission to update this product.");

        }

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Info = dto.Info;
        product.CategoryId = dto.CategoryId;
        product.ShippingType = dto.ShippingType;
        product.ShippingFee = dto.ShippingFee;
        product.AllowPickup = dto.AllowPickup;
        product.Status = dto.Status;

        if (dto.toRemoveImageIds != null && dto.toRemoveImageIds.Count > 0)
        {

            await _prodImage.DeleteProductImagesAsync(dto.toRemoveImageIds);

            foreach (var imgId in dto.toRemoveImageIds)
            {
                var img = product.Images.FirstOrDefault(i => i.ImgFileId == imgId);
                if (img != null)
                    product.Images.Remove(img);
            }

        }

        if (dto.newImages != null && dto.newImages.Count > 0)
        {

            await _prodImage.UploadProductImagesAsync(dto.newImages, productId, product.UserId);

        }

        var index = 0;
        foreach (var img in product.Images.OrderBy(i => i.ImgIndex))
        {
            img.ImgIndex = index++;
        }

        _productRepo.Update(product);
        await _productRepo.SaveAsync();

        return ToDto(product);

    }

    public async Task<List<ProductCardDto>> QueryProductCardsAsync(
        ProductFilter? filter,
        ProductOrder? order,
        int count)
    {

        IQueryable<Product> query = _productRepo.Query()
            .Where(p => p.Status == ProductStatus.Available);

        if (filter != null)
            query = filter(query);

        if (order != null)
            query = order(query);

        var products = await query
            .Take(count)
            .Include(p => p.Images)
            .Include(p => p.Seller)
            .ToListAsync();

        var ids = products.Select(p => p.ProductId);
        var viewCounts = await _productViewRepo.GetViewCountsAsync(ids);

        return products.Select(p =>
            ToProductCard(p, viewCounts.GetValueOrDefault(p.ProductId, 0))
        ).ToList();

    }

    public async Task<bool> DeleteAsync(long productId, int userId)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return false;

        if (product.UserId != userId)
        {
            
            throw new UnauthorizedAccessException("You do not have permission to delete this product.");

        }

        var imageIds = product.Images.Select(i => i.ImgFileId).ToList();
        if (imageIds.Count > 0)
            await _prodImage.DeleteProductImagesAsync(imageIds);
        product.Images.Clear();

        _productRepo.Delete(product);
        await _productRepo.SaveAsync();
        return true;

    }

    public static readonly ProductOrder Latest = q =>
        q.OrderByDescending(p => p.ReleaseDate);

    public static readonly ProductOrder Hottest = q =>
        q.OrderByDescending(p => p.Views.Count)
         .ThenByDescending(p => p.ReleaseDate);

    private static ProductDto ToDto(Product p, int viewCount = 0) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        Price = p.Price,
        Info = p.Info,
        Status = p.Status,
        ReleaseDate = p.ReleaseDate,
        UserId = p.UserId,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.CategoryName,
        ViewCount = viewCount,
        ShippingType = p.ShippingType,
        ShippingFee = p.ShippingFee,
        AllowPickup = p.AllowPickup,
        Images = p.Images?.Select(i => new ProductImageDto
        {
            ImgFileId = i.ImgFileId,
            ImgIndex = i.ImgIndex
        }).ToList() ?? new()
    };

    public static ProductCardDto ToProductCard(Product p, int viewCount = 0) => new()
    {
        ProductId = p.ProductId,
        Name = p.Name,
        Price = p.Price,
        CoverImageFileId = p.Images?
            .OrderBy(i => i.ImgIndex)
            .FirstOrDefault()?.ImgFileId,
        SellerName = p.Seller?.UserName ?? "",
        ReleaseDate = p.ReleaseDate,
        ViewCount = viewCount
    };

}
