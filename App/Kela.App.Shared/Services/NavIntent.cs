namespace Kela.App.Shared.Services;

public enum NavKind
{
    /// <summary>Dərinə keçid (kart → detal). Səhifə sağdan sürüşür.</summary>
    Forward,

    /// <summary>Geri qayıdış. Səhifə soldan sürüşür.</summary>
    Back,

    /// <summary>Alt/üst menyu sekmesi. Native-də animasiya olunmur.</summary>
    Tab
}

/// <summary>
/// Növbəti naviqasiyanın niyyətini daşıyır. Blazor naviqasiya istiqamətini
/// vermədiyi üçün niyyəti naviqasiyanı başladan komponent qeyd edir; keçid
/// animasiyasını seçən MainLayout onu oxuyub sıfırlayır.
/// </summary>
public sealed class NavIntent
{
    public NavKind Next { get; set; } = NavKind.Forward;

    public NavKind Take()
    {
        var value = Next;
        Next = NavKind.Forward;
        return value;
    }
}
