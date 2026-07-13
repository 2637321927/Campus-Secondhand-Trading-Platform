using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;

    public ProductService(IProductRepository productRepo)
    {
        _productRepo = productRepo;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _productRepo.GetAllAsync();
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(long productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        return product == null ? null : ToDto(product); 
    }

    public async Task<List<ProductDto>> GetAvailableAsync()
    {
        var products = await _productRepo.GetAvailableAsync();
        return products.Select(ToDto).ToList();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Info = dto.Info,
            Status = ProductStatus.Available,
            UserId = dto.UserId,
            CategoryId = dto.CategoryId,
            ReleaseDate = DateTime.Now
        };

        await _productRepo.AddAsync(product);
        await _productRepo.SaveAsync();

        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(long productId, UpdateProductDto dto)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return null;

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Info != null) product.Info = dto.Info;
        if (dto.Status.HasValue) product.Status = dto.Status.Value;

        _productRepo.Update(product);
        await _productRepo.SaveAsync();

        return ToDto(product);
    }

    public async Task<bool> DeleteAsync(long productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return false;

        _productRepo.Delete(product);
        await _productRepo.SaveAsync();
        return true;
    }

    private static ProductDto ToDto(Product p) => new()
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
        Images = p.Images?.Select(i => new ProductImageDto
        {
            ImgFileId = i.ImgFileId,
            ImgIndex = i.ImgIndex
        }).ToList() ?? new()
    };
}
