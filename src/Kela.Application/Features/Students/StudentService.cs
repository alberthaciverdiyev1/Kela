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
        int page, string? search, string? language, CancellationToken cancellationToken = default)
    {
        var lang = LanguageCodes.Normalize(language);
        var result = await students.GetPageAsync(page, search, cancellationToken);

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

    // ── Oluştur: mail + şifre sistem üretir, yanıtta düz metin döner ──
    public async Task<StudentCreatedResponse> CreateAsync(
        CreateStudentRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        await EnsureCityExistsAsync(request.CityId, cancellationToken);

        // E-posta kullanıcı verdiyse onu kullan, boşsa sistem üretir.
        var email = await ResolveEmailAsync(request.Email, cancellationToken);
        var password = StudentCredentialsGenerator.GeneratePassword();

        // User'ı Student rolüyle oluştur (Identity: hash, rol üyeliği).
        var userId = await auth.CreateUserAsync(new CreateUserRequest(
            request.FirstName, request.LastName, email, password, RoleNames.Student,
            request.PhoneNumber),
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

        // Düz metin şifre YALNIZCA bu yanıtta döner (öğretmen öğrenciye iletir).
        return new StudentCreatedResponse(profile.Id, userId, email, password, profile.CreatedAt);
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
            profile.User.SetPhoneNumber(request.PhoneNumber);
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

    private async Task<string> ResolveEmailAsync(string? email, CancellationToken cancellationToken)
    {
        // Kullanıcı e-posta verdiyse eşsizliğini doğrula, boşsa sistem üretir.
        if (string.IsNullOrWhiteSpace(email))
        {
            return await GenerateUniqueEmailAsync(cancellationToken);
        }

        var normalized = email.Trim().ToLowerInvariant();
        if (await userManager.FindByEmailAsync(normalized) is not null)
        {
            throw new InvalidOperationException($"'{normalized}' e-posta adresi zaten kayıtlı.");
        }
        return normalized;
    }

    private async Task<string> GenerateUniqueEmailAsync(CancellationToken cancellationToken)
    {
        // E-posta boş bırakılırsa sistem üretir. Eşsiz olmalı
        // (Identity RequireUniqueEmail) — çakışırsa yeniden üret.
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var email = StudentCredentialsGenerator.GenerateEmail();
            if (await userManager.FindByEmailAsync(email) is null)
            {
                return email;
            }
        }

        throw new InvalidOperationException("Eşsiz bir öğrenci maili üretilemedi. Tekrar deneyin.");
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
            p.User?.PhoneNumber,
            p.User?.Email ?? string.Empty,
            p.BirthDate,
            p.CityId,
            p.City is null ? null : LocalizedText.Get(p.City.NameTranslations, lang),
            p.CreatedAt);
}
