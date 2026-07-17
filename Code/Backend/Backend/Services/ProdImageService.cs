using Backend.Models;
using Backend.Repositories;

namespace Backend.Services;

public class ProdImageService : IProdImageService
{

    private readonly IUpdatedFileService _updatedFile;
    private readonly IProdImageRepository _prodImageRepo;

    public ProdImageService(IUpdatedFileService updatedFileService, IProdImageRepository prodImageRepo)
    {
        _updatedFile = updatedFileService;
        _prodImageRepo = prodImageRepo;
    }

    public async Task<List<ProdImage>> UploadProductImagesAsync(List<IFormFile> files, long productId, long uploaderId)
    {
        
        if (files == null || files.Count == 0)
        {
            
            throw new ArgumentException("At least one image is required to create a product.");

        }

        var uploadedFiles = await _updatedFile.UploadMultipleAsync(files, uploaderId);
        var productImages = new List<ProdImage>();

        foreach (var file in uploadedFiles)
        {
            
            var productImage = new ProdImage
            {

                ProductId = productId,
                ImgIndex = uploadedFiles.IndexOf(file),
                ImgFileId = file.FileId

            };

            productImages.Add(productImage);

        }

        try
        {
            
            foreach (var productImage in productImages)
            {
                
                await _prodImageRepo.AddAsync(productImage);

            }

            await _prodImageRepo.SaveAsync();

            return productImages;
            
        }
        catch
        {
            
            foreach (var file in uploadedFiles)
            {
                
                await _updatedFile.HardDeleteAsync(file.FileId);

            }

            throw;

        }

    }

    public async Task DeleteProductImagesAsync(List<long> imageIds)
    {

        if (imageIds == null || imageIds.Count == 0)
        {

            throw new ArgumentException("No image IDs provided for deletion.");

        }

        foreach (var imageId in imageIds)
        {
            
            var img = await _prodImageRepo.GetByIdAsync(imageId);
            if (img != null)
            {
                
                _prodImageRepo.Delete(img);

            }

        }

        await _prodImageRepo.SaveAsync();

        await _updatedFile.HardDeleteMultipleAsync(imageIds);

    }
    
}