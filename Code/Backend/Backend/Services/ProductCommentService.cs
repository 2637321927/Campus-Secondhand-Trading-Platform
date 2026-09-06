using Backend.Dtos.Product;
using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ProductCommentService : IProductCommentService
{

    private readonly IProductCommentRepository _commentRepo;
    private readonly IProductRepository _productRepo;

    public ProductCommentService(IProductCommentRepository commentRepo, IProductRepository productRepo)
    {

        _commentRepo = commentRepo;
        _productRepo = productRepo;

    }

    public async Task<List<ProductCommentDto>> GetByProductIdAsync(long productId)
    {

        var comments = await _commentRepo.GetByProductIdAsync(productId);

        var dict = comments.ToDictionary(c => c.CommentId, ToDto);
        var roots = new List<ProductCommentDto>();

        foreach (var c in comments)
        {

            var dto = dict[c.CommentId];
            if (c.ResponseToId is long parentId && dict.ContainsKey(parentId))
                dict[parentId].Replies.Add(dto);
            else
                roots.Add(dto);

        }

        return roots;

    }

    public async Task<ProductCommentDto> CreateAsync(long productId, int userId, CreateProductCommentDto dto)
    {

        if (await _productRepo.GetByIdAsync(productId) == null)
            throw new ArgumentException("Product does not exist.");

        if (dto.ResponseToId is long responseToId)
        {

            var parent = await _commentRepo.GetByIdAsync(responseToId);
            if (parent == null || parent.ProductId != productId)
                throw new ArgumentException("Target comment does not exist or does not belong to this product.");

        }

        var nextIndex = await _commentRepo.GetNextIndexAsync(productId);

        var comment = new ProductComment
        {

            ProductId = productId,
            UserId = userId,
            Content = dto.Content,
            ResponseToId = dto.ResponseToId,
            Index = nextIndex,
            CreateTime = DateTime.Now
            
        };

        await _commentRepo.AddAsync(comment);
        await _commentRepo.SaveAsync();

        return ToDto(comment);

    }

    public async Task<bool> DeleteAsync(long commentId, int userId)
    {

        var comment = await _commentRepo.GetByIdAsync(commentId);
        if (comment == null) return false;

        var product = await _productRepo.GetByIdAsync(comment.ProductId);
        if (product == null) return false;

        if (comment.UserId != userId && product.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to delete this comment.");

        _commentRepo.Delete(comment);
        await _commentRepo.SaveAsync();
        return true;

    }

    private static ProductCommentDto ToDto(ProductComment c) => new()
    {

        CommentId = c.CommentId,
        UserId = c.UserId,
        UserName = c.User?.UserName ?? "",
        Content = c.Content,
        Index = c.Index,
        ResponseToId = c.ResponseToId,
        CreateTime = c.CreateTime

    };

}
