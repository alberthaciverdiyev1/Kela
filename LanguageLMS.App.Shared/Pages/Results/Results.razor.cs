using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Results;

public partial class Results
{
    private bool _loading = true;
    private List<QuizResultSummaryDto> _results = new();
    private List<QuizResultSummaryDto> _filtered = new();

    private DateTime _from = DateTime.Today.AddDays(-30);
    private DateTime _to = DateTime.Today;

    private int _count;
    private int _avg;
    private int _best;

    protected override async Task OnInitializedAsync()
    {
        try { _results = await Api.ResultsAsync(); } catch { }
        Recompute();
        _loading = false;
    }

    private Task RecomputeAsync()
    {
        Recompute();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void Recompute()
    {
        var start = _from.Date;
        var end = _to.Date.AddDays(1);

        _filtered = _results
            .Where(r => r.StartDate is not null
                        && r.StartDate.Value.LocalDateTime >= start
                        && r.StartDate.Value.LocalDateTime < end)
            .OrderByDescending(r => r.StartDate)
            .ToList();

        _count = _filtered.Count;
        _avg = _count > 0 ? (int)Math.Round(_filtered.Average(r => r.ScorePercent)) : 0;
        _best = _count > 0 ? _filtered.Max(r => r.ScorePercent) : 0;
    }
}
