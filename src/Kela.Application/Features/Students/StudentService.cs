using FluentValidation;
using Kela.Application.Features.Cities;
using Kela.Application.Features.Students.Requests;
using Kela.Application.Features.Students.Responses;
using Kela.Application.Features.Users;
using Kela.Application.Features.Users.Auth;
using Kela.Application.Features.Users.Requests;
using Kela.Application.Pagination;
using Kela.Application.Patterns;
using Kela.Domain.Common;
using Kela.Domain.Entities;
using Kela.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Kela.Application.Features.Students;

internal sealed class StudentService(
    IStudentRepository students,
    IAuthService auth,
    ICityRepository cities,
    UserManager<User> userManager,
    IUnitOfWork unitOfWork,
    IValidator<CreateStudentRequest> createValidator,
    IValidator<UpdateStudentRequest> updateValidator) : IStudentService
{
    // ── Liste: User + City bilgisiyle, istenen dile göre şehir adı ──
    public async Task<PaginatedResult<StudentResponse>> GetPageAsync(
        int page, int pageSize, string? language, CancellationToken cancellationToken = default)
    {
        var lang = LanguageCodes.Normalize(language);
        var result = await students.GetPageAsync(page, pageSize, cancellationToken);

        return new PaginatedResult<StudentResponse>(
            result.Items.Select(ToResponse(lang)).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    public async Task<StudentResponse?> GetByIdAsync(
        int id, string? language, CancellationToken cancellationToken = default)
    {
        var lang = LanguageCodes.Normalize(language);
        var student = await students.GetByIdAsync(id, cancellationToken);

        return student is null ? null : ToResponse(lang)(student);
    }

    // ── Oluştur: Student rolünde User + bağlı StudentProfile ──
    public async Task<int> CreateAsync(
        CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        await EnsureCityExistsAsync(request.CityId, cancellationToken);

        // User'ı Student rolüyle oluştur (Identity: hash, rol üyeliği).
        var userId = await auth.CreateUserAsync(new CreateUserRequest(
            request.FirstName, request.LastName, request.Email, request.Password, RoleNames.Student),
            cancellationToken);

        var profile = new StudentProfile
        {
            UserId = userId,
            CityId = request.CityId,
            BirthDate = request.BirthDate,
            CreatedAt = DateTime.UtcNow,
        };

        students.Add(profile);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return profile.Id;
    }

    // ── Güncelle: User ad/soyad + StudentProfile şehir/doğum ──
    public async Task UpdateAsync(
        int id, UpdateStudentRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var profile = await students.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan öğrenci bulunamadı.");

        await EnsureCityExistsAsync(request.CityId, cancellationToken);

        if (profile.User is not null)
        {
            profile.User.SetName(request.FirstName, request.LastName);
            await userManager.UpdateAsync(profile.User);
        }

        profile.CityId = request.CityId;
        profile.BirthDate = request.BirthDate;
        profile.UpdatedAt = DateTime.UtcNow;

        students.Update(profile);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Sil: soft delete — User'ı Inactive, profili mantıken sil ──
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var profile = await students.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan öğrenci bulunamadı.");

        if (profile.User is not null)
        {
            profile.User.DeletedAt = DateTime.UtcNow;
            profile.User.SetStatus(UserStatus.Inactive);
            await userManager.UpdateAsync(profile.User);
        }

        profile.DeletedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;
        students.Remove(profile);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCityExistsAsync(int? cityId, CancellationToken cancellationToken)
    {
        if (cityId is not int id) return;

        if (await cities.GetByIdAsync(id, cancellationToken) is null)
        {
            throw new KeyNotFoundException($"Id = {id} olan şehir bulunamadı.");
        }
    }

    private static Func<StudentProfile, StudentResponse> ToResponse(string lang)
        => p => new StudentResponse(
            p.Id,
            p.UserId,
            p.User?.FirstName ?? string.Empty,
            p.User?.LastName ?? string.Empty,
            p.User?.Email ?? string.Empty,
            p.BirthDate,
            p.CityId,
            p.City is null ? null : LocalizedText.Get(p.City.NameTranslations, lang),
            p.CreatedAt);
}
