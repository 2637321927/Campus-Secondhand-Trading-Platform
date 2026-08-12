using Backend.Dtos.Product;

namespace Backend.Services;

public interface IProductCommentService
{

    Task<List<ProductCommentDto>> GetByProductIdAsync(long productId);
    Task<ProductCommentDto> CreateAsync(long productId, int userId, CreateProductCommentDto dto);
    Task<bool> DeleteAsync(long commentId, int userId);

}
