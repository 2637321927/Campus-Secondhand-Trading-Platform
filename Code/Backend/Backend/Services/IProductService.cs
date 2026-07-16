using Backend.Dtos.Product;
using Backend.Models;

namespace Backend.Services;

public delegate IQueryable<Product> ProductFilter(IQueryable<Product> query);

public delegate IQueryable<Product> ProductOrder(IQueryable<Product> query);

public interface IProductService
{

    Task<ProductDto?> GetByIdAsync(long productId, int userId);
    Task<ProductDto?> CreateAsync(int userId, CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(long productId, int userId, UpdateProductDto dto);
    Task<bool> DeleteAsync(long productId, int userId);
    Task RecordViewAsync(long productId, int userId);
    Task<List<ProductCardDto>> QueryProductCardsAsync(
        ProductFilter? filter,
        ProductOrder? order,
        int count);

}
