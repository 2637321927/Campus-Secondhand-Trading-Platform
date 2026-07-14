using Backend.Models;

namespace Backend.Services;

public interface IUpdatedFileService
{

    /// <summary>
    /// 批量上传文件
    /// </summary>
    Task<List<UpdatedFile>> UploadMultipleAsync(List<IFormFile> files, long uploaderId);

    /// <summary>
    /// 获取文件无论是否被软删除
    /// </summary>
    Task<UpdatedFile?> GetByIdAsync(long fileId);

    /// <summary>
    /// 获取未被软删除的文件记录
    /// </summary>
    Task<UpdatedFile?> GetActiveByIdAsync(long fileId);

    /// <summary>
    /// 将文件内容写入outputStream
    /// </summary>
    Task GetFileContentAsync(long fileId, Stream outputStream);

    /// <summary>
    /// 获取文件元数据与内容
    /// </summary>
    Task<UpdatedFile> GetFileStreamAsync(long fileId, Stream outputStream);

    /// <summary>
    /// 软删除
    /// </summary>
    Task SoftDeleteAsync(long fileId);

    /// <summary>
    /// 物理删除
    /// </summary>
    Task HardDeleteAsync(long fileId);

    /// <summary>
    /// 批量物理删除
    /// </summary>
    Task HardDeleteMultipleAsync(List<long> fileIds);
    
}