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

    public ProductService(IProductRepository productRepo, ICategoryRepository categoryRepo, IProductViewRepository productViewRepo, IProdImageService prodImageService)
    {
        _productRepo = productRepo;
        _categoryRepo = categoryRepo;
        _productViewRepo = productViewRepo;
        _prodImage = prodImageService;
    }

    public async Task<ProductDto?> GetByIdAsync(long productId, int userId)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;
        if (userId != -1 && product.UserId != userId) await RecordViewAsync(productId, userId);
        var viewCount = await _productViewRepo.GetViewCountAsync(productId);
        return ToDto(product, viewCount);

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

    public async Task<ProductDto?> CreateAsync(int userId, CreateProductDto dto)
    {

        if (await _categoryRepo.GetByIdAsync(dto.CategoryId) == null)
        {
            
            throw new ArgumentException("Category does not exist.");

        }

        var product = new Product
        {

            Name = dto.Name,
            Price = dto.Price,
            Info = dto.Info,
            Status = ProductStatus.Available,
            UserId = userId,
            ReleaseDate = DateTime.Now,
            CategoryId = dto.CategoryId
            
        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveAsync();

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
        product.Status = dto.Status;
        product.CategoryId = dto.CategoryId;

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
        Images = p.Images?.Select(i => new ProductImageDto
        {
            ImgFileId = i.ImgFileId,
            ImgIndex = i.ImgIndex
        }).ToList() ?? new()
    };

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
