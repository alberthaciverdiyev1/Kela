namespace Edutor.App.Shared.Dtos;

/// <summary>Şagirdin qrupu — progress backend-də hesablanıb gəlir.</summary>
public record GroupDto(
    long Id,
    string Name,
    string? Description,
    string? Image,
    int StudentCount,
    int TotalContents,
    int CompletedContents,
    int ProgressPercent)
{
    public bool HasImage => !string.IsNullOrWhiteSpace(Image)
                            && !Image.Contains("noPhoto", StringComparison.OrdinalIgnoreCase);

    public string Initial => string.IsNullOrWhiteSpace(Name) ? "?" : Name[..1].ToUpper();
}
