using Microsoft.JSInterop;

namespace Kela.App.Shared.Services.Localization;

/// <summary>Client-side lokalizasiya. Açarlar ingiliscə, dəyərlər seçilmiş dilə uyğun.</summary>
public sealed class Localizer(IJSRuntime js)
{
    private const string Key = "lms.lang";

    public static readonly string[] Supported = ["az", "en", "ru", "tr"];

    public string Current { get; private set; } = "az";

    public event Action? Changed;

    public string this[string key] => _map.TryGetValue(key, out var value) ? value : key;

    private Dictionary<string, string> _map = All["az"];

    public async Task InitAsync()
    {
        try
        {
            var saved = await js.InvokeAsync<string?>("localStorage.getItem", Key);
            if (!string.IsNullOrWhiteSpace(saved) && Supported.Contains(saved)) Current = saved;
        }
        catch { /* JS hazır deyil */ }

        _map = All.TryGetValue(Current, out var m) ? m : All["az"];
    }

    public async Task SetAsync(string lang)
    {
        if (!Supported.Contains(lang)) return;
        Current = lang;
        _map = All.TryGetValue(lang, out var m) ? m : All["az"];
        try { await js.InvokeVoidAsync("localStorage.setItem", Key, lang); } catch { /* noop */ }
        Changed?.Invoke();
    }

    private static readonly Dictionary<string, Dictionary<string, string>> All = new()
    {
        ["az"] = new()
        {
            ["App_Name"] = "Kela",
            ["Nav_Overview"] = "İcmal",
            ["Nav_Groups"] = "Qruplar",
            ["Nav_Exams"] = "İmtahanlar",
            ["Nav_Results"] = "Nəticələr",
            ["Nav_Notifications"] = "Bildirişlər",
            ["Nav_Profile"] = "Profil",
            ["Nav_Contact"] = "Əlaqə",
            ["Nav_Home"] = "Ana səhifə",
            ["Nav_Exam"] = "İmtahan",
        },
        ["en"] = new()
        {
            ["App_Name"] = "Kela",
            ["Nav_Overview"] = "Overview",
            ["Nav_Groups"] = "Groups",
            ["Nav_Exams"] = "Exams",
            ["Nav_Results"] = "Results",
            ["Nav_Notifications"] = "Notifications",
            ["Nav_Profile"] = "Profile",
            ["Nav_Contact"] = "Contact",
            ["Nav_Home"] = "Home",
            ["Nav_Exam"] = "Exam",
        },
        ["ru"] = new()
        {
            ["App_Name"] = "Kela",
            ["Nav_Overview"] = "Обзор",
            ["Nav_Groups"] = "Группы",
            ["Nav_Exams"] = "Экзамены",
            ["Nav_Results"] = "Результаты",
            ["Nav_Notifications"] = "Уведомления",
            ["Nav_Profile"] = "Профиль",
            ["Nav_Contact"] = "Контакты",
            ["Nav_Home"] = "Главная",
            ["Nav_Exam"] = "Экзамен",
        },
        ["tr"] = new()
        {
            ["App_Name"] = "Kela",
            ["Nav_Overview"] = "Genel bakış",
            ["Nav_Groups"] = "Gruplar",
            ["Nav_Exams"] = "Sınavlar",
            ["Nav_Results"] = "Sonuçlar",
            ["Nav_Notifications"] = "Bildirimler",
            ["Nav_Profile"] = "Profil",
            ["Nav_Contact"] = "İletişim",
            ["Nav_Home"] = "Ana sayfa",
            ["Nav_Exam"] = "Sınav",
        },
    };
}
