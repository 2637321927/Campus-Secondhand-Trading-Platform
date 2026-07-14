using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;
using Backend.Repositories;

namespace Backend.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepo;
    private readonly IUpdatedFileService _updatedFile;
    private readonly ICategoryRepository _categoryRepo;

    public ProductService(IProductRepository productRepo, IUpdatedFileService updatedFileService, ICategoryRepository categoryRepo)
    {
        _productRepo = productRepo;
        _updatedFile = updatedFileService;
        _categoryRepo = categoryRepo;
    }

    public async Task<ProductDto?> GetByIdAsync(long productId)
    {
        var product = await _productRepo.GetByIdAsync(productId);
        return product == null ? null : ToDto(product); 
    }
    public async Task<ProductDto> CreateAsync(int userId, CreateProductDto dto)
    {

        if (await _categoryRepo.GetByIdAsync(dto.CategoryId) == null)
        {
            
            throw new ArgumentException("Category does not exist.");

        }

        if (dto.Images == null || dto.Images.Count == 0)
        {
            
            throw new ArgumentException("At least one image is required to create a product.");

        }

        UploadMultipleResult result = await _updatedFile.UploadMultipleAsync(dto.Images, userId);

        if (result.Failed.Count != 0)
        {

            if (result.Succeeded.Count > 0)
            {
                
                foreach (var file in result.Succeeded)
                {
                    
                    await _updatedFile.SoftDeleteAsync(file.FileId);

                }

            }

            throw new InvalidOperationException("Failed to upload images.");

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

        try
        {

            await _productRepo.AddAsync(product);
            await _productRepo.SaveAsync();

        }
        catch
        {

            foreach (var file in result.Succeeded)
            {
                
                await _updatedFile.SoftDeleteAsync(file.FileId);

            }

            throw;

        }

        foreach (var file in result.Succeeded)
        {

            product.Images.Add(new ProdImage
            {
                ProductId = product.ProductId,
                ImgFileId = file.FileId,
                ImgIndex = product.Images.Count
            });

        }

        try
        {

            await _productRepo.SaveAsync();

        }
        catch
        {

            foreach (var file in result.Succeeded)
            {
                
                await _updatedFile.SoftDeleteAsync(file.FileId);

            }

            throw;

        }

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

    public async Task<bool> DeleteAsync(long productId, int userId)
    {

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null) return false;

        if (product.UserId != userId)
        {
            
            throw new UnauthorizedAccessException("You do not have permission to delete this product.");

        }

        foreach (var img in product.Images)
        {
            
            await _updatedFile.SoftDeleteAsync(img.ImgFileId);

        }

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
