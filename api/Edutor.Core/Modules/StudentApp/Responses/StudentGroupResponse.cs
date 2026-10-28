namespace Edutor.Core.Modules.StudentApp.Responses;

/// <summary>Şagird app üçün qrup — şagirdin həmin qrupdakı progressi backend-də hesablanmış gəlir.</summary>
public record StudentGroupResponse(
    long Id,
    string Name,
    string? Description,
    string? Image,
    int StudentCount,
    int TotalContents,
    int CompletedContents,
    int ProgressPercent);
