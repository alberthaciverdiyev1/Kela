namespace Kela.App.Shared.Dtos.Quiz;

public record QuizQuestionDto(
    long Id,
    string Title,
    string? VariantA,
    string? VariantB,
    string? VariantC,
    string? VariantD,
    string? VariantE,
    string TitleType = "text",
    string? TitleUrl = null,
    string VariantType = "text",
    string? VariantAUrl = null,
    string? VariantBUrl = null,
    string? VariantCUrl = null,
    string? VariantDUrl = null,
    string? VariantEUrl = null)
{
    public bool HasTitleImage => !string.IsNullOrWhiteSpace(TitleUrl);

    public IReadOnlyList<(string Key, string? Text, string? Url)> Options
    {
        get
        {
            var list = new List<(string, string?, string?)>();
            if (!string.IsNullOrWhiteSpace(VariantA) || !string.IsNullOrWhiteSpace(VariantAUrl)) list.Add(("A", VariantA, VariantAUrl));
            if (!string.IsNullOrWhiteSpace(VariantB) || !string.IsNullOrWhiteSpace(VariantBUrl)) list.Add(("B", VariantB, VariantBUrl));
            if (!string.IsNullOrWhiteSpace(VariantC) || !string.IsNullOrWhiteSpace(VariantCUrl)) list.Add(("C", VariantC, VariantCUrl));
            if (!string.IsNullOrWhiteSpace(VariantD) || !string.IsNullOrWhiteSpace(VariantDUrl)) list.Add(("D", VariantD, VariantDUrl));
            if (!string.IsNullOrWhiteSpace(VariantE) || !string.IsNullOrWhiteSpace(VariantEUrl)) list.Add(("E", VariantE, VariantEUrl));
            return list;
        }
    }
}
