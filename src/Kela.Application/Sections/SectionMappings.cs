using Kela.Application.Sections.Dtos;
using Kela.Domain.Entities;

namespace Kela.Application.Sections;

public static class SectionMappings
{
    public static SectionDto ToDto(this Section section) => new(
        section.Id,
        section.Name,
        section.Level,
        section.TeacherId,
        section.Teacher is null ? null : $"{section.Teacher.User.FirstName} {section.Teacher.User.LastName}",
        section.Students.Count,
        section.CreatedAt);
}
