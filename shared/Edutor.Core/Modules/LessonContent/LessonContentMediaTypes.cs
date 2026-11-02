using Edutor.Core.Modules.LessonContent.Entities;

namespace Edutor.Core.Modules.LessonContent;

/// <summary>
/// Material növünə uyğun fayl növləri: video material yalnız video, səs material yalnız
/// səs, "fayl" material isə video/səsdən başqa hər şeyi qəbul edir.
/// </summary>
public static class LessonContentMediaTypes
{
    private static readonly string[] VideoExtensions =
    [
        ".mp4", ".m4v", ".mov", ".mkv", ".webm", ".avi", ".wmv", ".flv",
        ".mpg", ".mpeg", ".3gp", ".ogv", ".ts", ".mts", ".m2ts", ".vob", ".mxf"
    ];

    private static readonly string[] AudioExtensions =
    [
        ".mp3", ".m4a", ".m4b", ".aac", ".wav", ".ogg", ".oga",
        ".opus", ".flac", ".wma", ".aiff", ".aif", ".amr", ".weba"
    ];

    /// <summary>
    /// Brauzerin fayl seçicisi üçün filtr. "Fayl" növündə filtr boşdur: brauzer
    /// "video/səsdən başqa hər şey" ifadəsini ifadə edə bilmir, seçim ayrıca yoxlanılır.
    /// </summary>
    public static string AcceptFor(LessonContentType type) => type switch
    {
        LessonContentType.Video => "video/*," + string.Join(",", VideoExtensions),
        LessonContentType.Voice => "audio/*," + string.Join(",", AudioExtensions),
        _ => string.Empty
    };

    public static IReadOnlyList<string> ExtensionsFor(LessonContentType type) => type switch
    {
        LessonContentType.Video => VideoExtensions,
        LessonContentType.Voice => AudioExtensions,
        _ => []
    };

    public static bool IsAllowed(LessonContentType type, string? fileName, string? contentType = null)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        var mime = contentType ?? string.Empty;

        var isVideo = VideoExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
            || mime.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
        var isAudio = AudioExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
            || mime.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

        return type switch
        {
            LessonContentType.Video => isVideo && !isAudio,
            LessonContentType.Voice => isAudio,
            _ => !isVideo && !isAudio
        };
    }

    public static void EnsureAllowed(LessonContentType type, string? fileName, string? contentType = null)
    {
        if (IsAllowed(type, fileName, contentType)) return;

        throw new ArgumentException(type switch
        {
            LessonContentType.Video => "Only video files can be uploaded for video material",
            LessonContentType.Voice => "Only audio files can be uploaded for voice material",
            _ => "Video and audio files must be uploaded as video or voice material"
        });
    }
}
