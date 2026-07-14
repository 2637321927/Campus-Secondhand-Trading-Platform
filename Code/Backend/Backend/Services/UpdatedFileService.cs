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

    public async Task<List<UpdatedFile>> UploadMultipleAsync(List<IFormFile> files, long uploaderId)
    {

        if (files == null || files.Count == 0)
            return new List<UpdatedFile>();

        var entities = new List<UpdatedFile>(files.Count);
        var storagePaths = new List<string>(files.Count);

        try
        {

            foreach (var file in files)
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
                storagePaths.Add(storagePath);

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

                await _repo.AddAsync(entity);
                entities.Add(entity);

            }

            await _repo.SaveAsync();
            return entities;

        }
        catch
        {

            foreach (var path in storagePaths)
                await _storage.DeleteFileAsync(path);

            throw;

        }

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

    public async Task HardDeleteMultipleAsync(List<long> fileIds)
    {

        if (fileIds == null || fileIds.Count == 0)
            return;

        var files = await _repo.GetByIdsAsync(fileIds);

        if (files.Count == 0)
            return;

        foreach (var file in files)
            await _storage.DeleteFileAsync(file.StoragePath);

        try
        {

            _repo.DeleteRange(files);
            await _repo.SaveAsync();

        }
        catch
        {

            foreach (var file in files)
                await _repo.SoftDeleteAsync(file.FileId);
            await _repo.SaveAsync();

        }

    }

}