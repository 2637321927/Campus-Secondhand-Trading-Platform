using Backend.Dtos.Product;

namespace Backend.Services;

public interface IProductService
{

    Task<ProductDto?> GetByIdAsync(long productId);
    Task<ProductDto> CreateAsync(int userId, CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(long productId, UpdateProductDto dto);
    Task<bool> DeleteAsync(long productId, int userId);

}
