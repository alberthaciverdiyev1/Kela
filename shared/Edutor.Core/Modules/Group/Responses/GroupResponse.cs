namespace Edutor.Core.Modules.Group.Responses;

public record GroupResponse(
    long Id,
    string Name,
    string? Description,
    string? Image,
    long? UserId,
    int StudentCount,
    string? JoinToken = null,
    /// <summary>Gözləyən qoşulma müraciətlərinin sayı.</summary>
    int PendingJoins = 0);

public record GroupJoinRequestResponse(
    long Id,
    long StudentId,
    string FullName,
    string Email,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DecidedAt);

public record GroupMemberResponse(
    long StudentId,
    string FullName,
    string Email,
    DateOnly? StartDate,
    DateOnly? EndDate);
