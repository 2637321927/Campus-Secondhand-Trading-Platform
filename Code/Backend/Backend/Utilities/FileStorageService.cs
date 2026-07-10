using Backend.Models.Enums;

namespace Backend.Utilities;

public class FileStorageService : IFileStorageService
{

    private readonly string _storagePath;

    public FileStorageService(IConfiguration configuration)
    {

        _storagePath = configuration.GetSection("FileStorage:StoragePath").Value ?? "./Uploads";

        if (!Directory.Exists(_storagePath))
        {

            Directory.CreateDirectory(_storagePath);

        }

    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {

        var contentPath = Path.Combine(_storagePath, ContentTypeMapper.FromMimeType(contentType).ToFolderName());

        if (!Directory.Exists(contentPath))
        {

            Directory.CreateDirectory(contentPath);

        }

        var filePath = Path.Combine(contentPath, fileName);

        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {

            await fileStream.CopyToAsync(fileStreamOutput);

        }

        return filePath;

    }

    public async Task DeleteFileAsync(string fileUrl)
    {

        if (File.Exists(fileUrl))
        {

            await Task.Run(() => File.Delete(fileUrl));

        }
        else
        {

            throw new FileNotFoundException("File not found.", fileUrl);

        }

    }

    public async Task GetFileAsync(string fileUrl, Stream outputStream)
    {

        if (File.Exists(fileUrl))
        {

            using (var fileStreamInput = new FileStream(fileUrl, FileMode.Open, FileAccess.Read))
            {

                await fileStreamInput.CopyToAsync(outputStream);

            }

        }
        else
        {

            throw new FileNotFoundException("File not found.", fileUrl);

        }

    }

    public async Task<string> GetFileMetadataAsync(string fileUrl)
    {

        if (File.Exists(fileUrl))
        {

            var fileInfo = new FileInfo(fileUrl);
            return $"Name: {fileInfo.Name}, Size: {fileInfo.Length} bytes, Created: {fileInfo.CreationTime}";

        }

        throw new FileNotFoundException("File not found.", fileUrl);

    }

}