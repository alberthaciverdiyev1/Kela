using LanguageLMS.Core.Modules.LessonContent.Entities;

namespace LanguageLMS.Core.Modules.LessonContent.Requests;

public class LessonContentUploadUrlRequest
{
    public long LessonId { get; set; }

    public LessonContentType Type { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = "application/octet-stream";
}
