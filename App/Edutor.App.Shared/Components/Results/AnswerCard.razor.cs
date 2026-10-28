using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Dtos.Quiz;

namespace Edutor.App.Shared.Components.Results;

public partial class AnswerCard
{
    [Parameter, EditorRequired] public QuizResultItemDto Item { get; set; } = default!;
    [Parameter] public int Number { get; set; }

    private List<(string Key, string Text)> Options
    {
        get
        {
            var list = new List<(string, string)>();
            if (!string.IsNullOrWhiteSpace(Item.VariantA)) list.Add(("A", Item.VariantA!));
            if (!string.IsNullOrWhiteSpace(Item.VariantB)) list.Add(("B", Item.VariantB!));
            if (!string.IsNullOrWhiteSpace(Item.VariantC)) list.Add(("C", Item.VariantC!));
            if (!string.IsNullOrWhiteSpace(Item.VariantD)) list.Add(("D", Item.VariantD!));
            if (!string.IsNullOrWhiteSpace(Item.VariantE)) list.Add(("E", Item.VariantE!));
            return list;
        }
    }

    private string OptClass(string key)
    {
        if (key == Item.CorrectAnswer) return "border-brand-149 bg-brand-081/50 text-brand-776";
        if (key == Item.StudentAnswer) return "border-danger-539/40 bg-danger-043 text-brand-776";
        return "border-brand-075 bg-white text-brand-755";
    }
}
