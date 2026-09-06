using Backend.Dtos.Product;
using Backend.Models;

namespace Backend.Services;

public interface IProdImageService
{

    /// <summary>
    /// 上传货物图片
    /// </summary>
    public Task<List<ProdImage>> UploadProductImagesAsync(List<IFormFile> files, long productId, long uploaderId);
    public Task DeleteProductImagesAsync(List<long> imageIds);

    /// <summary>
    /// 批量获取商品图片，按入参 fileIds 顺序返回
    /// </summary>
    public Task<List<ProductImageDataDto>> GetProductImagesAsync(List<long> fileIds);

}