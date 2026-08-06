using Kela.Application.Features.Sections.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Sections;

public static class SectionMappings
{
    public static SectionResponse ToResponse(this Section section) => new(
        section.Id,
        section.Name,
        section.Level,
        section.TeacherId,
        section.Teacher is null ? null : $"{section.Teacher.FirstName} {section.Teacher.LastName}",
        section.Students.Count,
        section.CreatedAt);
}
