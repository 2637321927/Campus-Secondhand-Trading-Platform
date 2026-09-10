using Backend.Dtos.Product;
using Backend.Models;
using Backend.Models.Enums;

namespace Backend.Services;

public delegate IQueryable<Product> ProductFilter(IQueryable<Product> query);

public delegate IQueryable<Product> ProductOrder(IQueryable<Product> query);

public interface IProductService
{

    Task<ProductDto?> GetByIdAsync(long productId, int userId);
    Task<List<ProductDto>> GetAllAsync();
    Task<List<ProductDto>> GetByStatusesAsync(IEnumerable<ProductStatus> statuses);
    Task<ProductDto?> CreateAsync(int userId, CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(long productId, int userId, UpdateProductDto dto);
    Task<ProductDto?> UpdateStatusAsync(long productId, int userId, ProductStatus target);
    Task<bool> DeleteAsync(long productId, int userId);
    Task RecordViewAsync(long productId, int userId);
    Task<List<long>> GetProductIdsByUserIdAsync(int userId);
    Task<List<ProductDto>> GetProductsByUserIdAsync(int userId);
    Task<List<ProductDto>> GetSoldProductsByUserIdAsync(int userId);
    Task<List<ProductCardDto>> QueryProductCardsAsync(
        ProductFilter? filter,
        ProductOrder? order,
        int count);

}
