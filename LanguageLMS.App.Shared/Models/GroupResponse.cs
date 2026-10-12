namespace LanguageLMS.App.Shared.Models;

/// <summary>Şagirdin qrupu — progress backend-də hesablanıb gəlir.</summary>
public record GroupResponse(
    long Id,
    string Name,
    string? Description,
    string? Image,
    int StudentCount,
    int TotalContents,
    int CompletedContents,
    int ProgressPercent)
{
    /// <summary>Real yüklənmiş şəkil var? (default "noPhoto" və boş halda false)</summary>
    public bool HasImage => !string.IsNullOrWhiteSpace(Image)
                            && !Image.Contains("noPhoto", StringComparison.OrdinalIgnoreCase);

    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name[..1].ToUpper();
}
