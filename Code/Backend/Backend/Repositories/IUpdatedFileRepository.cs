using Backend.Models;

namespace Backend.Repositories;

public interface IUpdatedFileRepository
{
    // ==================== 基础查询 ====================
    /// <summary>
    /// 按ID查询，无论是否被软删除都返回
    /// </summary>
    Task<UpdatedFile?> GetByIdAsync(long updatedFileId);

    /// <summary>
    /// 按ID查询，仅返回未被软删除的文件
    /// </summary>
    Task<UpdatedFile?> GetActiveByIdAsync(long updatedFileId);

    Task<List<UpdatedFile>> GetAllAsync();

    // ==================== 写操作 ====================
    Task AddAsync(UpdatedFile updatedFile);

    /// <summary>
    /// 批量添加文件记录
    /// </summary>
    Task AddRangeAsync(IEnumerable<UpdatedFile> files);

    void Update(UpdatedFile updatedFile);
    void Delete(UpdatedFile updatedFile);

    /// <summary>
    /// 批量删除文件记录（物理删除）
    /// </summary>
    void DeleteRange(IEnumerable<UpdatedFile> files);

    /// <summary>
    /// 软删除 — 标记 IsDeleted 并记录删除时间，不物理删除
    /// </summary>
    Task SoftDeleteAsync(long fileId);

    Task SaveAsync();
}