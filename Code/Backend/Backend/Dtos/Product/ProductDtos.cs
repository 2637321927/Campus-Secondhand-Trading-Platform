using Backend.Models.Enums;

namespace Backend.Dtos.Product;

public class CreateProductDto
{

    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Info { get; set; }
    public long CategoryId { get; set; }
    public List<IFormFile>? Images { get; set; }
    public ShippingType ShippingType { get; set; } = ShippingType.Free;
    public decimal? ShippingFee { get; set; }
    public int AllowPickup { get; set; } = 0;

}

public class UpdateProductDto
{

    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Info { get; set; }
    public long CategoryId { get; set; }
    public ShippingType ShippingType { get; set; } = ShippingType.Free;
    public decimal? ShippingFee { get; set; }
    public int AllowPickup { get; set; } = 0;
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    public List<IFormFile>? newImages { get; set; }
    public List<long>? toRemoveImageIds { get; set; } = new();

}

public class ProductDto
{
    public long ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Info { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Available;
    public DateTime ReleaseDate { get; set; }
    public int UserId { get; set; }
    public long CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int ViewCount { get; set; }
    public ShippingType ShippingType { get; set; }
    public decimal? ShippingFee { get; set; }
    public int AllowPickup { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public long ImgFileId { get; set; }
    public int ImgIndex { get; set; }
}

/// <summary>
/// 批量获取图片的返回 DTO
/// </summary>
public class ProductImageDataDto
{
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

///<summary>
///商品卡片信息响应（用于主页展示）
///</summary>
public class ProductCardDto
{
    public long ProductId { get; set; }
    public string Name { get; set; }= string.Empty;
    public decimal Price { get; set; }
    public long? CoverImageFileId { get; set; }
    public string SellerName { get; set; }= string.Empty;
    public DateTime ReleaseDate { get; set; }
    public int ViewCount { get; set; }
}
