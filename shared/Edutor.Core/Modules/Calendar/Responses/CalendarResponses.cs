namespace Edutor.Core.Modules.Calendar.Responses;

/// <summary>
/// Təqvim hadisəsi: müəllimin öz hadisəsi (redaktə olunur) və ya ev tapşırığının
/// son tarixi (yalnız oxunur). Vaxtı olmayan hadisələr gün boyu göstərilir.
/// </summary>
public record CalendarEventResponse(
    long? Id,
    DateOnly Date,
    string Type,
    string Title,
    string? Subtitle,
    string? Url,
    string Color,
    bool IsAllDay,
    DateTimeOffset? StartAt,
    DateTimeOffset? EndAt);

public record TeacherEventResponse(
    long Id,
    string Title,
    string? Description,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt,
    string Color);
