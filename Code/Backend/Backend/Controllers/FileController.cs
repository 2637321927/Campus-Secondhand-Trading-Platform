using Backend.Dtos.File;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/files")]
public class FileController : ControllerBase
{
    private readonly IUpdatedFileService _fileService;

    public FileController(IUpdatedFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// 上传商品图片
    /// </summary>
    [Authorize]
    [HttpPost("product-images")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadProductImages([FromForm] List<IFormFile> files)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var uploaded = await _fileService.UploadMultipleAsync(files, userId);
        return Ok(uploaded.Select(ToUploadResponseDto).ToList());
    }

    /// <summary>
    /// 上传用户头像
    /// </summary>
    [Authorize]
    [HttpPost("avatars")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadAvatar([FromForm] List<IFormFile> files)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var uploaded = await _fileService.UploadMultipleAsync(files, userId);
        return Ok(uploaded.Select(ToUploadResponseDto).ToList());
    }

    /// <summary>
    /// 上传会话附件
    /// </summary>
    [Authorize]
    [HttpPost("message-attachments")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadMessageAttachments([FromForm] List<IFormFile> files)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var uploaded = await _fileService.UploadMultipleAsync(files, userId);
        return Ok(uploaded.Select(ToUploadResponseDto).ToList());
    }

    /// <summary>
    /// 上传举报证据附件
    /// </summary>
    [Authorize]
    [HttpPost("report-attachments")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadReportAttachments([FromForm] List<IFormFile> files)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var uploaded = await _fileService.UploadMultipleAsync(files, userId);
        return Ok(uploaded.Select(ToUploadResponseDto).ToList());
    }

    /// <summary>
    /// 上传申诉材料附件
    /// </summary>
    [Authorize]
    [HttpPost("appeal-attachments")]
    public async Task<ActionResult<List<FileUploadResponseDto>>> UploadAppealAttachments([FromForm] List<IFormFile> files)
    {
        var userId = int.Parse(User.FindFirst("userId")!.Value);
        var uploaded = await _fileService.UploadMultipleAsync(files, userId);
        return Ok(uploaded.Select(ToUploadResponseDto).ToList());
    }

    /// <summary>
    /// 获取文件元信息
    /// </summary>
    [HttpGet("{fileId}/metadata")]
    public async Task<ActionResult<FileMetadataDto>> GetFileMetadata(long fileId)
    {
        var file = await _fileService.GetActiveByIdAsync(fileId);
        if (file == null) return NotFound();
        return Ok(ToMetadataDto(file));
    }

    /// <summary>
    /// 获取或下载文件
    /// </summary>
    [HttpGet("{fileId}")]
    public async Task<ActionResult> GetFile(long fileId)
    {
        try
        {
            var file = await _fileService.GetActiveByIdAsync(fileId);
            if (file == null) return NotFound();

            var memoryStream = new MemoryStream();
            await _fileService.GetFileContentAsync(fileId, memoryStream);
            memoryStream.Position = 0;

            return File(memoryStream, file.MimeType, file.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// 删除文件（软删除）
    /// </summary>
    [Authorize]
    [HttpDelete("{fileId}")]
    public async Task<ActionResult> DeleteFile(long fileId)
    {
        try
        {
            await _fileService.SoftDeleteAsync(fileId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static FileUploadResponseDto ToUploadResponseDto(Models.UpdatedFile f) => new()
    {
        FileId = f.FileId,
        FileName = f.FileName,
        MimeType = f.MimeType,
        FileSize = f.FileSize,
        ContentType = f.ContentType.ToString(),
        UploadTime = f.UploadTime
    };

    private static FileMetadataDto ToMetadataDto(Models.UpdatedFile f) => new()
    {
        FileId = f.FileId,
        FileName = f.FileName,
        MimeType = f.MimeType,
        FileSize = f.FileSize,
        ContentType = f.ContentType.ToString(),
        UploadTime = f.UploadTime,
        UploaderId = f.UploaderId
    };
}
