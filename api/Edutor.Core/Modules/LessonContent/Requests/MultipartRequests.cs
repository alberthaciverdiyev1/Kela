using Edutor.Core.Common.Storage;
namespace Edutor.Core.Modules.LessonContent.Requests;

/// <summary>Hissə nömrələri üçün presigned URL sorğusu.</summary>
public class MultipartPartsRequest
{
    public string Path { get; set; } = string.Empty;

    public string UploadId { get; set; } = string.Empty;

    /// <summary>Hansı hissələr üçün URL lazımdır (1-dən başlayır).</summary>
    public List<int> PartNumbers { get; set; } = [];
}

public class MultipartCompleteRequest
{
    public string Path { get; set; } = string.Empty;

    public string UploadId { get; set; } = string.Empty;

    /// <summary>Hər hissənin ETag-i — R2 CompleteMultipartUpload tələb edir.</summary>
    public List<MultipartPartResult> Parts { get; set; } = [];
}

public class MultipartAbortRequest
{
    public string Path { get; set; } = string.Empty;

    public string UploadId { get; set; } = string.Empty;
}
