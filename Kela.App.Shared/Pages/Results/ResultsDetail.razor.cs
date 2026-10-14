using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Results;

public partial class ResultsDetail
{
    [Parameter] public long AttemptId { get; set; }

    private bool _loading = true;
    private QuizResultDetailDto? _detail;

    private int Wrong => _detail is null ? 0 : Math.Max(0, _detail.TotalQuestions - _detail.CorrectCount);
    private int Score => _detail?.CorrectCount ?? 0;

    private string Level => (_detail?.ScorePercent ?? 0) >= 70 ? "Yüksək" : (_detail?.ScorePercent ?? 0) >= 40 ? "Orta" : "Aşağı";

    private string DateLabel => _detail?.StartDate?.ToLocalTime().ToString("dd.MM.yyyy HH:mm") ?? "—";

    private string DurationLabel
    {
        get
        {
            if (_detail?.StartDate is null || _detail.EndDate is null) return "—";
            var t = _detail.EndDate.Value - _detail.StartDate.Value;
            return t.TotalHours >= 1
                ? $"{(int)t.TotalHours} saat {t.Minutes} dəq"
                : t.TotalMinutes >= 1 ? $"{t.Minutes} dəq {t.Seconds} san" : $"{t.Seconds} san";
        }
    }

    protected override async Task OnInitializedAsync()
    {
        try { _detail = await Api.ResultDetailAsync(AttemptId); } catch { }
        _loading = false;
    }
}
