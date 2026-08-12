using Backend.Models;

namespace Backend.Repositories;

public interface IProductCommentRepository
{
    Task<ProductComment?> GetByIdAsync(long commentId);
    Task<List<ProductComment>> GetByProductIdAsync(long productId);
    Task<List<ProductComment>> GetRepliesAsync(long commentId);
    Task<int> GetNextIndexAsync(long productId);
    Task<bool> HasRepliesAsync(long commentId);
    Task AddAsync(ProductComment comment);
    void Delete(ProductComment comment);
    Task SaveAsync();
}
