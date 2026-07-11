namespace Backend.Utilities;

public interface IFileStorageService
{
    
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileUrl);
    Task GetFileAsync(string fileUrl, Stream outputStream);
    
}