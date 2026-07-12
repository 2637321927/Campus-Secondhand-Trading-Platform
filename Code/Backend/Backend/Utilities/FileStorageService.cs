using Backend.Models.Enums;

namespace Backend.Utilities;

public class FileStorageService : IFileStorageService
{

    private readonly string _storagePath;

    public FileStorageService(IConfiguration configuration)
    {

        _storagePath = configuration.GetSection("FileStorage:StoragePath").Value ?? "./Uploads";

        if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);

    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, FileType fileType)
    {

        var contentPath = Path.Combine(
            _storagePath,
            fileType.ToFolderName(),
            DateTime.Now.ToString("yyyy-MM-dd"));

        if (!Directory.Exists(contentPath)) Directory.CreateDirectory(contentPath);

        var ext = Path.GetExtension(fileName);
        var storedName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(contentPath, storedName);

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

}