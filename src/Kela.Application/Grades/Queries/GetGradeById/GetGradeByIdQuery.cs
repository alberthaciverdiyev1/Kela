using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Grades.Dtos;

namespace Kela.Application.Grades.Queries.GetGradeById;

public sealed record GetGradeByIdQuery(int Id) : IQuery<GradeDto?>;
