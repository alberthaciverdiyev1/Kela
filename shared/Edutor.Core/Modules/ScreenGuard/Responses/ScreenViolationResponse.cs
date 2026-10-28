namespace Edutor.Core.Modules.ScreenGuard.Responses;

public record ScreenViolationResponse(
    long Id,
    string Type,
    string? Path,
    string? UserAgent,
    DateTimeOffset CreatedAt);
