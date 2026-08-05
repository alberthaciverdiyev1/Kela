using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Sections.Dtos;
using Kela.Application.Repositories;

namespace Kela.Application.Sections.Queries.GetSectionById;

internal sealed class GetSectionByIdQueryHandler(ISectionRepository sections)
    : IQueryHandler<GetSectionByIdQuery, SectionDto?>
{
    public async Task<SectionDto?> Handle(GetSectionByIdQuery query, CancellationToken cancellationToken)
    {
        var section = await sections.GetByIdAsync(query.Id, cancellationToken);
        return section?.ToDto();
    }
}
