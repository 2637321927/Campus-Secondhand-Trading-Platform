using Backend.Models;
using Backend.Repositories;
using Backend.Utilities;
using Microsoft.Extensions.Configuration;

namespace Backend.Services;

public class UpdatedFileService : IUpdatedFileService
{

    private readonly IUpdatedFileRepository _repo;
    private readonly IFileStorageService _storage;
    private readonly IConfiguration _config;

    public UpdatedFileService(
        IUpdatedFileRepository updatedFileRepository,
        IFileStorageService fileStorageService,
        IConfiguration configuration)
    {

        _repo = updatedFileRepository;
        _storage = fileStorageService;
        _config = configuration;

    }

    public async Task<UpdatedFile> UploadAsync(IFormFile file, long uploaderId)
    {

        var fileType = ContentTypeMapper.FromMimeType(file.ContentType);
        var sizeLimit = _config.GetValue<long>($"FileStorage:SizeLimits:{fileType}");
        if (sizeLimit > 0 && file.Length > sizeLimit)
        {

            var sizeMb = file.Length / (1024.0 * 1024.0);
            var limitMb = sizeLimit / (1024.0 * 1024.0);
            throw new InvalidOperationException(
                $"文件 \"{file.FileName}\" 大小 {sizeMb:F2} MB 超过 {fileType} 类型上限 {limitMb:F2} MB。");
                
        }

        using var stream = file.OpenReadStream();
        var storagePath = await _storage.UploadFileAsync(stream, file.FileName, fileType);

        var entity = new UpdatedFile
        {

            FileName = file.FileName,
            StoragePath = storagePath,
            MimeType = file.ContentType,
            FileSize = file.Length,
            ContentType = fileType,
            UploaderId = (int)uploaderId,
            UploadTime = DateTime.Now
            
        };

        try
        {

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();
            return entity;

        }
        catch
        {

            await _storage.DeleteFileAsync(storagePath);
            throw;

        }

    }

    public async Task<UploadMultipleResult> UploadMultipleAsync(List<IFormFile> files, long uploaderId)
    {

        var succeeded = new List<UpdatedFile>(files.Count);
        var failed = new List<(string FileName, string Error)>();

        foreach (var file in files)
        {
            try
            {
                succeeded.Add(await UploadAsync(file, uploaderId));
            }
            catch (Exception ex)
            {
                failed.Add((file.FileName, ex.Message));
            }
        }

        return new UploadMultipleResult(succeeded, failed);

    }

    public async Task<UpdatedFile?> GetByIdAsync(long fileId)
        => await _repo.GetByIdAsync(fileId);

    public async Task<UpdatedFile?> GetActiveByIdAsync(long fileId)
        => await _repo.GetActiveByIdAsync(fileId);

    public async Task GetFileContentAsync(long fileId, Stream outputStream)
    {

        var file = await _repo.GetActiveByIdAsync(fileId)
            ?? throw new KeyNotFoundException($"UpdatedFile {fileId} not found or has been deleted.");

        await _storage.GetFileAsync(file.StoragePath, outputStream);

    }

    public async Task<UpdatedFile> GetFileStreamAsync(long fileId, Stream outputStream)
    {

        var metadata = await GetActiveByIdAsync(fileId)
            ?? throw new KeyNotFoundException($"UpdatedFile {fileId} not found or has been deleted.");

        await GetFileContentAsync(fileId, outputStream);

        return metadata;
        
    }

    public async Task SoftDeleteAsync(long fileId)
        => await _repo.SoftDeleteAsync(fileId);

    public async Task HardDeleteAsync(long fileId)
    {
        var file = await _repo.GetByIdAsync(fileId)
            ?? throw new KeyNotFoundException($"UpdatedFile {fileId} not found.");

        await _storage.DeleteFileAsync(file.StoragePath);

        try
        {

            _repo.Delete(file);
            await _repo.SaveAsync();

        }
        catch
        {

            await _repo.SoftDeleteAsync(fileId);
            await _repo.SaveAsync();
            throw;

        }
    }

}

/// <summary>
/// 批量上传结果
/// </summary>
/// <param name="Succeeded">上传成功的文件列表</param>
/// <param name="Failed">上传失败的文件及原因（FileName, Error）</param>
public record UploadMultipleResult(List<UpdatedFile> Succeeded, List<(string FileName, string Error)> Failed);