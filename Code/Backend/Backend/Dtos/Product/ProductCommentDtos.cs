namespace Backend.Dtos.Product;

public class CreateProductCommentDto
{
    public string Content { get; set; } = string.Empty;
    public long? ResponseToId { get; set; }
}

public class ProductCommentDto
{
    public long CommentId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Index { get; set; }
    public long? ResponseToId { get; set; }
    public DateTime CreateTime { get; set; }
    public List<ProductCommentDto> Replies { get; set; } = new();
}
