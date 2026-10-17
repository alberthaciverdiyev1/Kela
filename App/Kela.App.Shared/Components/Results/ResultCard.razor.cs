using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Components.Results;

public partial class ResultCard
{
    [Parameter, EditorRequired] public QuizResultSummaryDto Result { get; set; } = default!;

    private int Wrong => Math.Max(0, Result.TotalQuestions - Result.CorrectCount);
    private int Score => Result.CorrectCount;

    private string Level => Result.ScorePercent >= 70 ? L["Result_LevelHigh"] : Result.ScorePercent >= 40 ? L["Result_LevelMedium"] : L["Result_LevelLow"];
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
                ? L.Format("Time_HoursMinutes", (int)t.TotalHours, t.Minutes)
                : t.TotalMinutes >= 1 ? L.Format("Time_MinutesSeconds", t.Minutes, t.Seconds) : L.Format("Time_Seconds", t.Seconds);
        }
    }
}
