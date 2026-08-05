using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Grades.Dtos;
using Kela.Application.Repositories;

namespace Kela.Application.Grades.Queries.GetGrades;

internal sealed class GetGradesQueryHandler(IGradeRepository grades)
    : IQueryHandler<GetGradesQuery, List<GradeDto>>
{
    public async Task<List<GradeDto>> Handle(GetGradesQuery query, CancellationToken cancellationToken)
    {
        var all = await grades.GetAllAsync(cancellationToken);
        return all.Select(g => g.ToDto()).ToList();
    }
}
