using Backend.Models.Enums;

namespace Backend.Utilities;

public static class ContentTypeMapper
{

    private static readonly HashSet<string> DocumentMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {

        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "text/plain",

    };

    public static FileType FromMimeType(string mimeType)
    {

        if (string.IsNullOrWhiteSpace(mimeType))
            return FileType.Other;

        if (DocumentMimeTypes.Contains(mimeType))
            return FileType.Document;

        var prefix = mimeType.Split('/')[0].ToLowerInvariant();

        return prefix switch
        {

            "image" => FileType.Image,
            "video" => FileType.Video,
            _ => FileType.Other

        };

    }

    public static string ToFolderName(this FileType fileType) => fileType switch
    {

        FileType.Image => "images",
        FileType.Video => "videos",
        FileType.Document => "documents",
        FileType.Other => "other",
        _ => "other"
        
    };

}
