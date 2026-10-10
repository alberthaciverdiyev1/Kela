namespace LanguageLMS.Core.Modules.Group.Responses;

public record GroupResponse(
    long Id,
    string Name,
    string? Description,
    string? Image,
    long? UserId,
    int StudentCount);

public record GroupMemberResponse(
    long StudentId,
    string FullName,
    string Email,
    DateOnly? StartDate,
    DateOnly? EndDate);
