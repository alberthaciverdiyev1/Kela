namespace Kela.Application.Features.Cities.Responses;

/// <summary>
/// Şehir detayı — yerelleştirilmiş ad + tüm dillerdeki adlar (jsonb sözlük).
/// Yönetim/edit formu için <see cref="Translations"/> sözlüğü olduğu gibi döner.
/// </summary>
public sealed record CityResponse(
    int Id,
    string Language,
    string Name,
    IReadOnlyDictionary<string, string> Translations,
    DateTime CreatedAt);
