using Kela.Application.Abstractions.Cqrs;
using Kela.Application.Sections.Dtos;

namespace Kela.Application.Sections.Queries.GetSectionById;

public sealed record GetSectionByIdQuery(int Id) : IQuery<SectionDto?>;
