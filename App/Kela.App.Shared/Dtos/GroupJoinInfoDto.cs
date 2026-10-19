namespace Kela.App.Shared.Dtos;

/// <summary>Dəvət linki ilə qrup məlumatı və müraciət vəziyyəti.</summary>
public record GroupJoinInfoDto(
    long GroupId,
    string Name,
    string? Description,
    string? Image,
    string? TeacherName,
    /// <summary>none | pending | approved | rejected | member | anonymous</summary>
    string Status,
    long TenantId = 0);
