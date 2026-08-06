using Kela.Application.Features.Sections.Requests;
using Kela.Application.Features.Sections.Responses;
using Kela.Application.Features.Users;
using Kela.Application.Pagination;
using Kela.Application.Patterns;
using Kela.Application.Validation;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Sections;

internal sealed class SectionService(
    ISectionRepository sections,
    IUserRepository users,
    IUnitOfWork unitOfWork,
    IValidator<CreateSectionRequest> createValidator,
    IValidator<UpdateSectionRequest> updateValidator) : ISectionService
{
    public async Task<PaginatedResult<SectionResponse>> GetPageAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await sections.GetPageAsync(page, pageSize, cancellationToken);
        return new PaginatedResult<SectionResponse>(
            result.Items.Select(s => s.ToResponse()).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<SectionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var section = await sections.GetByIdAsync(id, cancellationToken);
        return section?.ToResponse();
    }

    public async Task<int> CreateAsync(
        CreateSectionRequest request, CancellationToken cancellationToken = default)
    {
        createValidator.Validate(request);

        var trimmed = request.Name.Trim();

        if (await sections.NameExistsAsync(trimmed, cancellationToken))
        {
            throw new InvalidOperationException($"'{trimmed}' adlı sınıf zaten kayıtlı.");
        }

        if (request.TeacherId is int id)
        {
            await EnsureTeacherExistsAsync(id, cancellationToken);
        }

        var section = new Section
        {
            Name = trimmed,
            Level = request.Level,
            TeacherId = request.TeacherId,
            CreatedAt = DateTime.UtcNow,
        };

        sections.Add(section);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return section.Id;
    }

    public async Task UpdateAsync(
        int id, UpdateSectionRequest request, CancellationToken cancellationToken = default)
    {
        updateValidator.Validate(request);

        var section = await sections.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan sınıf bulunamadı.");

        var trimmed = request.Name.Trim();
        if (trimmed != section.Name && await sections.NameExistsAsync(trimmed, cancellationToken))
        {
            throw new InvalidOperationException($"'{trimmed}' adlı sınıf zaten kayıtlı.");
        }

        if (request.TeacherId is int teacher)
        {
            await EnsureTeacherExistsAsync(teacher, cancellationToken);
        }

        section.Name = trimmed;
        section.Level = request.Level;
        section.TeacherId = request.TeacherId;
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
        if (user is null || user.Teacher is null)
        {
            throw new InvalidOperationException($"Id = {teacherId} olan öğretmen bulunamadı.");
        }
    }
}
