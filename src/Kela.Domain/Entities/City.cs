using Kela.Domain.Common;

namespace Kela.Domain.Entities;

/// <summary>
/// Şehir kaydı. Adlar dile göre ayrı bir tabloda değil, tek bir JSON
/// (jsonb) sütununda saklanır: { "az": "Bakı", "en": "Baku", ... }.
/// Gelecekte çevirisi olacak diğer entity'ler de aynı Dictionary + jsonb
/// desenini kullanır — ayrı tablo/junction yönetimi gerekmez.
/// </summary>
public class City : BaseEntity
{
    /// <summary>Dile göre adlar (LanguageCodes.All anahtarları).</summary>
    public Dictionary<string, string> NameTranslations { get; set; } = new();
}
