namespace Kela.Application.Grades.Dtos;

public sealed record GradeDto(
    int Id,
    string Name,
    int Level,
    int? TeacherId,
    string? TeacherName,
    int StudentCount,
    DateTime CreatedAt);
