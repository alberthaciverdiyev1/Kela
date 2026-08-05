using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Sections.Dtos;
using Kela.Application.Pagination;

namespace Kela.Application.Sections.Queries.GetSections;

public sealed record GetSectionsQuery(int Page = 1, int PageSize = 20) : IQuery<PaginatedResult<SectionDto>>;
