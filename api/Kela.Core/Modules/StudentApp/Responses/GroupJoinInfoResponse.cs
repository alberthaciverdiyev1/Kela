namespace Kela.Core.Modules.StudentApp.Responses;

public record GroupJoinInfoResponse(
    long GroupId,
    string Name,
    string? Description,
    string? Image,
    string? TeacherName,
    /// <summary>none | pending | approved | rejected | member</summary>
    string Status);
