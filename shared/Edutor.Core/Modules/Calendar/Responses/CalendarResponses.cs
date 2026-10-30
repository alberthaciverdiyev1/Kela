namespace Edutor.Core.Modules.Calendar.Responses;

/// <summary>Təqvim hadisəsi: ev tapşırığının son tarixi və ya şəxsi qeyd.</summary>
public record CalendarEventResponse(
    DateOnly Date,
    string Type,
    string Title,
    string? Subtitle,
    string? Url,
    bool IsDone);
