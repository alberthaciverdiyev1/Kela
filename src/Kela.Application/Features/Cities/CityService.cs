using FluentValidation;
using Kela.Application.Features.Cities.Requests;
using Kela.Application.Features.Cities.Responses;
using Kela.Application.Pagination;
using Kela.Application.Patterns;
using Kela.Domain.Common;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Cities;

internal sealed class CityService(
    ICityRepository cities,
    IUnitOfWork unitOfWork,
    IValidator<CreateCityRequest> createValidator,
    IValidator<UpdateCityRequest> updateValidator) : ICityService
{
    // ── Liste: istenen dile göre yerelleştirilmiş ad ──
    public async Task<PaginatedResult<CityListItemResponse>> GetPageAsync(
        int page, string? language, CancellationToken cancellationToken = default)
    {
        var lang = LanguageCodes.Normalize(language);
        var result = await cities.GetPageAsync(page, cancellationToken);

        return new PaginatedResult<CityListItemResponse>(
            result.Items.Select(c => new CityListItemResponse(
                c.Id, lang, LocalizedText.Get(c.NameTranslations, lang), c.CreatedAt)).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount);
    }

    // ── Detay: yerelleştirilmiş ad + yönetim için tüm diller ──
    public async Task<CityResponse?> GetByIdAsync(
        int id, string? language, CancellationToken cancellationToken = default)
    {
        var lang = LanguageCodes.Normalize(language);
        var city = await cities.GetByIdAsync(id, cancellationToken);

        return city is null ? null : new CityResponse(
            city.Id,
            lang,
            LocalizedText.Get(city.NameTranslations, lang),
            city.NameTranslations,
            city.CreatedAt);
    }

    // ── Oluştur: tek JSON sözlük alınır ──
    public async Task<int> CreateAsync(CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var city = new City
        {
            NameTranslations = NormalizeTranslations(request.Translations),
            CreatedAt = DateTime.UtcNow,
        };

        cities.Add(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return city.Id;
    }

    // ── Güncelle: sözlüğün tamamı değiştirilir ──
    public async Task UpdateAsync(int id, UpdateCityRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var city = await cities.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan şehir bulunamadı.");

        city.NameTranslations = NormalizeTranslations(request.Translations);
        city.UpdatedAt = DateTime.UtcNow;

        cities.Update(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ── Sil ──
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var city = await cities.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan şehir bulunamadı.");

        cities.Remove(city);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, string> NormalizeTranslations(IReadOnlyDictionary<string, string> translations)
        => translations.ToDictionary(kv => LanguageCodes.Normalize(kv.Key), kv => kv.Value.Trim());
}
