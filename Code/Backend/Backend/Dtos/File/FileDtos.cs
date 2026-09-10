namespace Backend.Dtos.File;

/// <summary>
/// 文件上传响应
/// </summary>
public class FileUploadDto
{
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
