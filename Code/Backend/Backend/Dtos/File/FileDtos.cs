namespace Backend.Dtos.File;

/// <summary>
/// 文件上传成功后返回的 DTO
/// </summary>
public class FileUploadResponseDto
{
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadTime { get; set; }
}

/// <summary>
/// 文件元信息 DTO
/// </summary>
public class FileMetadataDto
{
    public long FileId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadTime { get; set; }
    public int UploaderId { get; set; }
}
