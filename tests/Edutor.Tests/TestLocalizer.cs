using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;

/// <summary>Test üçün sadə lokalizator — açarı olduğu kimi qaytarır.</summary>
public sealed class TestLocalizer : IStringLocalizer<SharedResource>
{
    public LocalizedString this[string name] => new(name, name);

    public LocalizedString this[string name, params object[] arguments]
        => new(name, string.Format(name, arguments));

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
}
