namespace LanguageLMS.App.Shared.Models;

public record GroupResponse(
    long Id,
    string Name,
    string? Description,
    string? Image,
    long? UserId,
    int StudentCount);