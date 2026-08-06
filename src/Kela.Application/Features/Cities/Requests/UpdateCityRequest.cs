namespace Kela.Application.Features.Cities.Requests;

/// <summary>
/// Mevcut şehri güncelleme isteği — tüm dillerdeki adlar tek JSON nesnesi
/// olarak gönderilir ve sözlüğün tamamı değiştirilir.
/// </summary>
public sealed record UpdateCityRequest(Dictionary<string, string> Translations);
