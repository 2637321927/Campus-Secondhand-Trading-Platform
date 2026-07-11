using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Backend.Models.Enums;

namespace Backend.Models;

[Table("files")]
public class UpdatedFile
{

    [Key]
    [Column("file_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long FileId { get; set; }

    [Column("file_name")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Column("storage_path")]
    [MaxLength(500)]
    public string StoragePath { get; set; } = string.Empty;

    [Column("mime_type")]
    [MaxLength(100)]
    public string MimeType { get; set; } = string.Empty;

    [Column("file_size")]
    public long FileSize { get; set; }

    [Column("content_type")]
    public FileType ContentType { get; set; } = FileType.Other;

    [Column("upload_time")]
    public DateTime UploadTime { get; set; } = DateTime.Now;

    [Column("uploader_id")]
    public int UploaderId { get; set; }

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [Column("deleted_time")]
    public DateTime? DeletedTime { get; set; }

    [ForeignKey("UploaderId")]
    public BaseUser Uploader { get; set; } = null!;

}