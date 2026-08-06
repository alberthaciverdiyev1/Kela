using Kela.Domain.Common;
using Kela.Domain.Entities;

namespace Kela.Infrastructure.Data.Seeds;

public static class CitySeed
{
    public static readonly (string Az, string En, string Ru, string Tr)[] Items =
    {
        ("Bakı",     "Baku",     "Баку",      "Bakü"),
        ("Gəncə",    "Ganja",    "Гянджа",    "Gence"),
        ("Moskva",   "Moscow",   "Москва",    "Moskova"),
        ("İstanbul", "Istanbul", "Стамбул",   "İstanbul"),
        ("Ankara",   "Ankara",   "Анкара",    "Ankara"),
        ("London",   "London",   "Лондон",    "Londra"),
        ("Paris",    "Paris",    "Париж",     "Paris"),
        ("Berlin",   "Berlin",   "Берлин",    "Berlin"),
        ("Barselona","Barcelona","Барселона", "Barselona"),
        ("Roma",     "Rome",     "Рим",       "Roma"),
    };

    public static City[] Build()
    {
        var now = DateTime.UtcNow;

        return Items.Select(item => new City
        {
            NameTranslations = new Dictionary<string, string>
            {
                [LanguageCodes.Az] = item.Az,
                [LanguageCodes.En] = item.En,
                [LanguageCodes.Ru] = item.Ru,
                [LanguageCodes.Tr] = item.Tr,
            },
            CreatedAt = now,
        }).ToArray();
    }
}
