namespace Kela.Core.Modules.Students.Responses;

public record StudentListItemResponse(
    long Id,
    string FullName,
    string Email,
    string? Phone,
    bool IsActive,
    DateTimeOffset CreatedAt);
