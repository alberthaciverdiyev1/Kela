using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Sections.Dtos;
using Kela.Application.Pagination;
using Kela.Application.Repositories;

namespace Kela.Application.Sections.Queries.GetSections;

internal sealed class GetSectionsQueryHandler(ISectionRepository sections)
    : IQueryHandler<GetSectionsQuery, PaginatedResult<SectionDto>>
{
    public async Task<PaginatedResult<SectionDto>> Handle(GetSectionsQuery query, CancellationToken cancellationToken)
    {
        var result = await sections.GetPageAsync(query.Page, query.PageSize, cancellationToken);
        return new PaginatedResult<SectionDto>(
            result.Items.Select(g => g.ToDto()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}
