namespace Kela.App.Shared.Dtos;

public record LessonDto(long Id, string Title, int SortOrder, string? Image = null)
{
    public bool HasImage => !string.IsNullOrWhiteSpace(Image)
                            && !Image.Contains("noPhoto", StringComparison.OrdinalIgnoreCase);

    public string Initial => string.IsNullOrWhiteSpace(Title) ? "?" : Title[..1].ToUpper();
}
