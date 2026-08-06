namespace Kela.Application.Features.Sections.Responses;

public sealed record SectionResponse(
    int Id,
    string Name,
    int Level,
    int? TeacherId,
    string? TeacherName,
    int StudentCount,
    DateTime CreatedAt);
