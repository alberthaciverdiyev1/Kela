namespace Kela.Core.Modules.LessonContent.Requests;

/// <summary>
/// Yükləmə bitdikdən sonra faylı mövcud materiala bağlayır. Fayl əvvəlcə fon
/// rejimində yüklənir, ona görə qeyd əvvəlcə mediasız yaranır və sonra bu sorğu ilə
/// tamamlanır.
/// </summary>
public class LessonContentAttachRequest
{
    public string Path { get; set; } = string.Empty;

    public Entities.LessonContentType Type { get; set; }
}
