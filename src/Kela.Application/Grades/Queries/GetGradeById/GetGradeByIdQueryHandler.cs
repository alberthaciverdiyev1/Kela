using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Grades.Dtos;
using Kela.Application.Repositories;

namespace Kela.Application.Grades.Queries.GetGradeById;

internal sealed class GetGradeByIdQueryHandler(IGradeRepository grades)
    : IQueryHandler<GetGradeByIdQuery, GradeDto?>
{
    public async Task<GradeDto?> Handle(GetGradeByIdQuery query, CancellationToken cancellationToken)
    {
        var grade = await grades.GetByIdAsync(query.Id, cancellationToken);
        return grade?.ToDto();
    }
}
