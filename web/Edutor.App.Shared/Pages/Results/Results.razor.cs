using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Results;

public partial class Results
{
    private bool _loading = true;
    private List<QuizResultSummaryDto> _filtered = new();

    private DateTime _from = DateTime.Today.AddDays(-30);
    private DateTime _to = DateTime.Today;

    private int _count;
    private int _avg;
    private int _best;

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    /// <summary>
    /// Seçilmiş tarix aralığı üzrə nəticələri serverdən çəkir. Süzgəc API-də tətbiq olunur.
    /// </summary>
    private async Task FetchAsync()
    {
        try
        {
            var from = new DateTimeOffset(_from.Date, TimeZoneInfo.Local.GetUtcOffset(_from));
            var to = new DateTimeOffset(_to.Date.AddDays(1), TimeZoneInfo.Local.GetUtcOffset(_to));
            _filtered = await Api.ResultsAsync(from, to);
        }
        catch
        {
            _filtered = new();
        }

        Recompute();
    }

    private void Recompute()
    {
        _count = _filtered.Count;
        _avg = _count > 0 ? (int)Math.Round(_filtered.Average(r => r.ScorePercent)) : 0;
        _best = _count > 0 ? _filtered.Max(r => r.ScorePercent) : 0;
    }
}
