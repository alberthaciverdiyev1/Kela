using Kela.Application.Sections.Responses;
using Kela.Domain.Entities;

namespace Kela.Application.Sections;

public static class SectionMappings
{
    public static SectionResponse ToResponse(this Section section) => new(
        section.Id,
        section.Name,
        section.Level,
        section.TeacherId,
        section.Teacher is null ? null : $"{section.Teacher.User.FirstName} {section.Teacher.User.LastName}",
        section.Students.Count,
        section.CreatedAt);
}
