namespace Edutor.Core.Modules.StudentApp.Responses;

public record GroupJoinInfoResponse(
    long GroupId,
    string Name,
    string? Description,
    string? Image,
    string? TeacherName,
    /// <summary>none | pending | approved | rejected | member | anonymous</summary>
    string Status,
    /// <summary>Qrupun aid olduğu tenant — qeydiyyat üçün.</summary>
    long TenantId);
