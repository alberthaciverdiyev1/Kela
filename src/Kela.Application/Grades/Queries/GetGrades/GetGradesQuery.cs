using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Grades.Dtos;

namespace Kela.Application.Grades.Queries.GetGrades;

public sealed record GetGradesQuery : IQuery<List<GradeDto>>;
