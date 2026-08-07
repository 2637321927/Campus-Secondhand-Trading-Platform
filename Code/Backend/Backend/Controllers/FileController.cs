using Backend.Repositories;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Backend.Controllers;

/// <summary>
/// 文件服务控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IUpdatedFileService _fileService;
    private readonly IBaseUserRepository _baseUserRepo;
    private readonly long _defaultAvatarFileId;

    public FileController(IUpdatedFileService fileService, IBaseUserRepository baseUserRepo, IConfiguration configuration)
    {
        _fileService = fileService;
        _baseUserRepo = baseUserRepo;
        _defaultAvatarFileId = configuration.GetValue<long>("AppDefaults:DefaultAvatarFileId");
    }

    /// <summary>
    /// 根据文件ID获取文件内容
    /// </summary>
    [HttpGet("{fileId:long}")]
    public async Task<IActionResult> GetFile(long fileId)
    {
        try
        {
            var stream = new MemoryStream();
            var metadata = await _fileService.GetFileStreamAsync(fileId, stream);

            stream.Position = 0;
            return File(stream, metadata.MimeType, metadata.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("文件不存在或已被删除");
        }
    }

    /// <summary>
    /// 根据文件ID删除文件
    /// </summary>
    [HttpDelete("{fileId:long}")]
    public async Task<IActionResult> DeleteFile(long fileId)
    {
        try
        {
            // 1. 先清理用户头像引用（将引用此文件的用户重置为默认头像）
            var users = await _baseUserRepo.GetAllAsync();
            bool avatarReset = false;
            foreach (var user in users)
            {
                if (user.AvatarFileId.HasValue && user.AvatarFileId.Value == fileId)
                {
                    user.AvatarFileId = _defaultAvatarFileId;
                    _baseUserRepo.Update(user);
                    avatarReset = true;
                }
            }
            if (avatarReset)
                await _baseUserRepo.SaveAsync();

            // 2. 再删除文件
            await _fileService.HardDeleteAsync(fileId);

            return Ok(new { message = "文件删除成功" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound("文件不存在或已被删除");
        }
    }
}
