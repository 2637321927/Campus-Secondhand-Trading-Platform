using Backend.Models.Enums;
using Microsoft.AspNetCore.Hosting;

namespace Backend.Utilities;

public class FileStorageService : IFileStorageService
{

    private readonly string _storagePath;
    private readonly string _contentRootPath;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {

        _contentRootPath = environment.ContentRootPath;
        var configuredPath = configuration.GetSection("FileStorage:StoragePath").Value ?? "./Uploads";
        _storagePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(_contentRootPath, configuredPath);

        if (!Directory.Exists(_storagePath)) Directory.CreateDirectory(_storagePath);

    }

    private string ResolvePath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;
        return Path.GetFullPath(Path.Combine(_contentRootPath, path));
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

        var resolvedPath = ResolvePath(fileUrl);

        if (File.Exists(resolvedPath))
        {

            await Task.Run(() => File.Delete(resolvedPath));

        }
        else
        {

            throw new FileNotFoundException("File not found.", fileUrl);

        }

    }

    public async Task GetFileAsync(string fileUrl, Stream outputStream)
    {

        var resolvedPath = ResolvePath(fileUrl);

        if (File.Exists(resolvedPath))
        {

            using (var fileStreamInput = new FileStream(resolvedPath, FileMode.Open, FileAccess.Read))
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
