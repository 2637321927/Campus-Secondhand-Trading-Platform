using Backend.Models;

namespace Backend.Services;

public interface IProdImageService
{
    
    /// <summary>
    /// 上传货物图片
    /// </summary>
    public Task<List<ProdImage>> UploadProductImagesAsync(List<IFormFile> files, long productId, long uploaderId);
    public Task DeleteProductImagesAsync(List<long> imageIds);

}