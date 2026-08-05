using Kela.Application.Grades.Dtos;
using Kela.Domain.Grades;

namespace Kela.Application.Grades;

public static class GradeMappings
{
    public static GradeDto ToDto(this Grade grade) => new(
        grade.Id,
        grade.Name,
        grade.Level,
        grade.TeacherId,
        grade.Teacher is null ? null : $"{grade.Teacher.User.FirstName} {grade.Teacher.User.LastName}",
        grade.Students.Count,
        grade.CreatedAt);
}
