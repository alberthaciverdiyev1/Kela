namespace Kela.Core.Modules.LessonContent.Responses;

public record LessonContentResponse(
    long Id,
    long LessonId,
    string Title,
    string? Description,
    string? Type,
    string? Url,
    string? PlaybackUrl,
    int SortOrder,
    bool IsActive,
    int? DurationSeconds,
    string? ThumbnailUrl = null,
    /// <summary>Video konversiya vəziyyəti (pending/processing/ready/failed). Video deyilsə null.</summary>
    string? ConversionStatus = null);
