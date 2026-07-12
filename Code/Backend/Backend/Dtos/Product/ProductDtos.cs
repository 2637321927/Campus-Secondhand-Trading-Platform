namespace Backend.Dtos.Product;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Info { get; set; }
    public int UserId { get; set; }
    public long CategoryId { get; set; }
}

public class UpdateProductDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Info { get; set; }
    public string? Status { get; set; }
}

public class ProductDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Info { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public int UserId { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public long ImgFileId { get; set; }
    public int ImgIndex { get; set; }
}
