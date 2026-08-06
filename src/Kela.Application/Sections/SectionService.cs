using Kela.Application.Pagination;
using Kela.Application.Repositories;
using Kela.Application.Sections.Dtos;
using Kela.Domain.Entities;
using Kela.Domain.Enums;

namespace Kela.Application.Sections;

internal sealed class SectionService(
    ISectionRepository sections,
    IUserRepository users,
    IUnitOfWork unitOfWork) : ISectionService
{
    public async Task<PaginatedResult<SectionDto>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await sections.GetPageAsync(page, pageSize, cancellationToken);
        return new PaginatedResult<SectionDto>(
            result.Items.Select(s => s.ToDto()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<SectionDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var section = await sections.GetByIdAsync(id, cancellationToken);
        return section?.ToDto();
    }

    public async Task<int> CreateAsync(
        string name, int level, int? teacherId, CancellationToken cancellationToken = default)
    {
        var trimmed = name.Trim();

        if (await sections.NameExistsAsync(trimmed, cancellationToken))
        {
            throw new InvalidOperationException($"'{trimmed}' adlı sınıf zaten kayıtlı.");
        }

        if (teacherId is int id)
        {
            await EnsureTeacherExistsAsync(id, cancellationToken);
        }

        var section = new Section
        {
            Name = trimmed,
            Level = level,
            TeacherId = teacherId,
            CreatedAt = DateTime.UtcNow,
        };

        sections.Add(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return section.Id;
    }

    public async Task UpdateAsync(
        int id, string name, int level, int? teacherId, CancellationToken cancellationToken = default)
    {
        var section = await sections.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan sınıf bulunamadı.");

        var trimmed = name.Trim();
        if (trimmed != section.Name && await sections.NameExistsAsync(trimmed, cancellationToken))
        {
            throw new InvalidOperationException($"'{trimmed}' adlı sınıf zaten kayıtlı.");
        }

        if (teacherId is int teacher)
        {
            await EnsureTeacherExistsAsync(teacher, cancellationToken);
        }

        section.Name = trimmed;
        section.Level = level;
        section.TeacherId = teacherId;
        section.UpdatedAt = DateTime.UtcNow;

        sections.Update(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var section = await sections.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan sınıf bulunamadı.");

        sections.Remove(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureTeacherExistsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(teacherId, cancellationToken);
        if (user is null || user.Role != Role.Teacher)
        {
            throw new InvalidOperationException($"Id = {teacherId} olan öğretmen bulunamadı.");
        }
    }
}
