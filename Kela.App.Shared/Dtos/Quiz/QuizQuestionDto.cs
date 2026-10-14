namespace Kela.App.Shared.Dtos.Quiz;

public record QuizQuestionDto(long Id, string Title, string? VariantA, string? VariantB, string? VariantC,
    string? VariantD, string? VariantE)
{
    public IReadOnlyList<(string Key, string Text)> Options
    {
        get
        {
            var list = new List<(string, string)>();
            if (!string.IsNullOrWhiteSpace(VariantA)) list.Add(("A", VariantA!));
            if (!string.IsNullOrWhiteSpace(VariantB)) list.Add(("B", VariantB!));
            if (!string.IsNullOrWhiteSpace(VariantC)) list.Add(("C", VariantC!));
            if (!string.IsNullOrWhiteSpace(VariantD)) list.Add(("D", VariantD!));
            if (!string.IsNullOrWhiteSpace(VariantE)) list.Add(("E", VariantE!));
            return list;
        }
    }
}
