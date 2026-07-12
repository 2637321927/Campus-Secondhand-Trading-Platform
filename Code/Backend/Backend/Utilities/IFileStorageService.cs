using Backend.Models.Enums;

namespace Backend.Utilities;

public interface IFileStorageService
{

    Task<string> UploadFileAsync(Stream fileStream, string fileName, FileType fileType);
    Task DeleteFileAsync(string fileUrl);
    Task GetFileAsync(string fileUrl, Stream outputStream);
    
}