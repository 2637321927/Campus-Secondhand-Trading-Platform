using Backend.Dtos.Product;

namespace Backend.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(long productId);
    Task<List<ProductDto>> GetAvailableAsync();
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(long productId, UpdateProductDto dto);
    Task<bool> DeleteAsync(long productId);
}
