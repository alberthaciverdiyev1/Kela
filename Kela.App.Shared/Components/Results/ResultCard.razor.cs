using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Components.Results;

public partial class ResultCard
{
    [Parameter, EditorRequired] public QuizResultSummaryDto Result { get; set; } = default!;

    private int Wrong => Math.Max(0, Result.TotalQuestions - Result.CorrectCount);
    private int Score => Result.CorrectCount;

    private string Level => Result.ScorePercent >= 70 ? "Yüksək" : Result.ScorePercent >= 40 ? "Orta" : "Aşağı";
    private string LevelClass => Result.ScorePercent >= 70
        ? "bg-brand-081 text-brand-671"
        : Result.ScorePercent >= 40 ? "bg-sun-063 text-sun-461" : "bg-danger-043 text-danger-539";

    private string DateLabel => Result.StartDate?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "—";

    private string DurationLabel
    {
        get
        {
            if (Result.DurationSeconds is not > 0) return "—";
            var t = TimeSpan.FromSeconds(Result.DurationSeconds.Value);
            return t.TotalHours >= 1
                ? $"{(int)t.TotalHours} saat {t.Minutes} dəq"
                : t.TotalMinutes >= 1 ? $"{t.Minutes} dəq {t.Seconds} san" : $"{t.Seconds} san";
        }
    }
}
