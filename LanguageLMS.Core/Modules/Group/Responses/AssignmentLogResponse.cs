namespace LanguageLMS.Core.Modules.Group.Responses;

public record AssignmentLogResponse(long Id, long SubjectId, long GroupId, string Action, DateTimeOffset CreatedAt);
