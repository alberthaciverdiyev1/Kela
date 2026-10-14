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

    public string TypeLabel(string? type) => this[type switch
    {
        "video" => "Type_Video",
        "file" => "Type_File",
        "voice" => "Type_Voice",
        _ => "Type_Material"
    }];

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
            ["Common_Loading"] = "Yüklənir...",
            ["Common_LoadingAria"] = "Yüklənir",
            ["Common_PleaseWait"] = "Bir az gözləyin",
            ["Common_Close"] = "Bağla",
            ["Common_Back"] = "Geri",
            ["Common_General"] = "Ümumi",
            ["Group_StudentsCount"] = "{0} tələbə",
            ["Group_StudentsCountShort"] = "{0} şagird",
            ["Group_ViewLessons"] = "Dərslərə bax",
            ["Continue_Next"] = "Növbəti: {0}",
            ["Quiz_Meta"] = "{0} sual · {1} dəq",
            ["Result_Correct"] = "Doğru",
            ["Result_Wrong"] = "Yanlış",
            ["Result_Questions"] = "Sual",
            ["Result_Score"] = "Bal: {0}",
            ["Result_Details"] = "Ətraflı",
            ["Result_LevelHigh"] = "Yüksək",
            ["Result_LevelMedium"] = "Orta",
            ["Result_LevelLow"] = "Aşağı",
            ["Answer_Your"] = "Sizin",
            ["Answer_NotAnswered"] = "Cavab verilməyib",
            ["Type_Video"] = "Video",
            ["Type_File"] = "Fayl",
            ["Type_Voice"] = "Səs yazısı",
            ["Type_Material"] = "Material",
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
            ["Common_Loading"] = "Loading...",
            ["Common_LoadingAria"] = "Loading",
            ["Common_PleaseWait"] = "Please wait a moment",
            ["Common_Close"] = "Close",
            ["Common_Back"] = "Back",
            ["Common_General"] = "General",
            ["Group_StudentsCount"] = "{0} students",
            ["Group_StudentsCountShort"] = "{0} students",
            ["Group_ViewLessons"] = "View lessons",
            ["Continue_Next"] = "Next: {0}",
            ["Quiz_Meta"] = "{0} questions · {1} min",
            ["Result_Correct"] = "Correct",
            ["Result_Wrong"] = "Wrong",
            ["Result_Questions"] = "Questions",
            ["Result_Score"] = "Score: {0}",
            ["Result_Details"] = "Details",
            ["Result_LevelHigh"] = "High",
            ["Result_LevelMedium"] = "Medium",
            ["Result_LevelLow"] = "Low",
            ["Answer_Your"] = "Your",
            ["Answer_NotAnswered"] = "Not answered",
            ["Type_Video"] = "Video",
            ["Type_File"] = "File",
            ["Type_Voice"] = "Audio",
            ["Type_Material"] = "Material",
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
            ["Common_Loading"] = "Загрузка...",
            ["Common_LoadingAria"] = "Загрузка",
            ["Common_PleaseWait"] = "Подождите немного",
            ["Common_Close"] = "Закрыть",
            ["Common_Back"] = "Назад",
            ["Common_General"] = "Общее",
            ["Group_StudentsCount"] = "{0} учеников",
            ["Group_StudentsCountShort"] = "{0} учеников",
            ["Group_ViewLessons"] = "Смотреть уроки",
            ["Continue_Next"] = "Далее: {0}",
            ["Quiz_Meta"] = "{0} вопросов · {1} мин",
            ["Result_Correct"] = "Верно",
            ["Result_Wrong"] = "Неверно",
            ["Result_Questions"] = "Вопросы",
            ["Result_Score"] = "Балл: {0}",
            ["Result_Details"] = "Подробнее",
            ["Result_LevelHigh"] = "Высокий",
            ["Result_LevelMedium"] = "Средний",
            ["Result_LevelLow"] = "Низкий",
            ["Answer_Your"] = "Ваш",
            ["Answer_NotAnswered"] = "Нет ответа",
            ["Type_Video"] = "Видео",
            ["Type_File"] = "Файл",
            ["Type_Voice"] = "Аудио",
            ["Type_Material"] = "Материал",
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
            ["Common_Loading"] = "Yükleniyor...",
            ["Common_LoadingAria"] = "Yükleniyor",
            ["Common_PleaseWait"] = "Biraz bekleyin",
            ["Common_Close"] = "Kapat",
            ["Common_Back"] = "Geri",
            ["Common_General"] = "Genel",
            ["Group_StudentsCount"] = "{0} öğrenci",
            ["Group_StudentsCountShort"] = "{0} öğrenci",
            ["Group_ViewLessons"] = "Dersleri gör",
            ["Continue_Next"] = "Sıradaki: {0}",
            ["Quiz_Meta"] = "{0} soru · {1} dk",
            ["Result_Correct"] = "Doğru",
            ["Result_Wrong"] = "Yanlış",
            ["Result_Questions"] = "Soru",
            ["Result_Score"] = "Puan: {0}",
            ["Result_Details"] = "Ayrıntılar",
            ["Result_LevelHigh"] = "Yüksek",
            ["Result_LevelMedium"] = "Orta",
            ["Result_LevelLow"] = "Düşük",
            ["Answer_Your"] = "Sizin",
            ["Answer_NotAnswered"] = "Cevap verilmedi",
            ["Type_Video"] = "Video",
            ["Type_File"] = "Dosya",
            ["Type_Voice"] = "Ses kaydı",
            ["Type_Material"] = "Materyal",
        },
    };
}
