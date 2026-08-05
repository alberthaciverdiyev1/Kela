namespace Kela.Application.Sections.Dtos;

public sealed record SectionDto(
    int Id,
    string Name,
    int Level,
    int? TeacherId,
    string? TeacherName,
    int StudentCount,
    DateTime CreatedAt);
