using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// 文件获取与删除（验证性接口）
/// </summary>
[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly IUpdatedFileService _fileService;
    private readonly IBaseUserRepository _userRepo;
    private readonly IConfiguration _configuration;

    public FileController(
        IUpdatedFileService fileService,
        IBaseUserRepository userRepo,
        IConfiguration configuration)
    {
        _fileService = fileService;
        _userRepo = userRepo;
        _configuration = configuration;
    }

    /// <summary>
    /// 按 ID 获取/下载文件
    /// </summary>
    [HttpGet("{fileId:long}")]
    public async Task<IActionResult> GetFile(long fileId)
    {
        var meta = await _fileService.GetActiveByIdAsync(fileId);
        if (meta == null)
            return NotFound(new { error = "文件不存在或已被删除" });

        var ms = new MemoryStream();
        try
        {
            await _fileService.GetFileContentAsync(fileId, ms);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "文件物理存储不存在" });
        }

        ms.Position = 0;
        return File(ms, meta.MimeType, meta.FileName);
    }

    /// <summary>
    /// 按 ID 删除文件，同时自动清理引用该文件作为头像的用户（重置为默认头像）
    /// </summary>
    [Authorize]
    [HttpDelete("{fileId:long}")]
    public async Task<IActionResult> DeleteFile(long fileId)
    {
        var meta = await _fileService.GetByIdAsync(fileId);
        if (meta == null)
            return NotFound(new { error = "文件不存在" });

        // 1. 先重置引用该文件作为头像的用户，避免外键悬空
        var defaultAvatarId = _configuration.GetValue<long?>("AppDefaults:DefaultAvatarFileId") ?? 1L;
        await _userRepo.ResetAvatarByFileIdAsync(fileId, defaultAvatarId);
        await _userRepo.SaveAsync();

        // 2. 删除文件记录与物理文件
        try
        {
            await _fileService.HardDeleteAsync(fileId);
        }
        catch (FileNotFoundException)
        {
            // 物理文件已不存在，尝试软删除 DB 记录
            try { await _fileService.SoftDeleteAsync(fileId); }
            catch { /* 忽略：记录可能已被清理 */ }
        }

        return NoContent();
    }
}
