using Backend.Dtos.Product;

namespace Backend.Services;

public interface IProductService
{

    Task<bool> CreateProductAsync(CreateProductDto dto);
    Task<ProductDto?> GetByIdAsync(long productId);
    Task<List<ProductDto>> GetAvailableAsync();
    Task<ProductDto> CreateAsync(CreateProductDto dto, int userId);
    Task<ProductDto?> UpdateAsync(long productId, UpdateProductDto dto);
    Task<bool> DeleteAsync(long productId);

}
