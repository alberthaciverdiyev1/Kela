namespace Kela.Application.Features.Sections.Responses;

/// <summary>Sınıf bilgisi (yalnızca okuma amaçlı yanıt).</summary>
public sealed record SectionResponse(
    int Id,
    string Name,
    int Level,
    int? TeacherId,
    string? TeacherName,
    int StudentCount,
    DateTime CreatedAt);
