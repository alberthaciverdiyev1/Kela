namespace Kela.Application.Features.Cities.Requests;

/// <summary>
/// Yeni şehir oluşturma isteği. Çeviriler tek JSON nesnesinde gelir:
/// { "translations": { "az": "Bakı", "en": "Baku", "ru": "Баку", "tr": "Bakü" } }
/// Gelecekte çevirisi olacak diğer entity'ler aynı Dictionary desenini kullanır.
/// </summary>
public sealed record CreateCityRequest(Dictionary<string, string> Translations);
