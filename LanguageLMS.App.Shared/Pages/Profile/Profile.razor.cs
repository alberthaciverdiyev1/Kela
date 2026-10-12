using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Profile;

public partial class Profile
{
    private int _groups;
    private int _quizzes;
    private int _completed;
    private int _totalContents;
    private int _completionPercent;
    private int _attempts;
    private int _averageScore;
    private int _bestScore;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _groups = (await Api.StudentGroupsAsync()).Count;
            _quizzes = (await Api.QuizzesAsync()).Count;

            var stats = await Api.StatisticsAsync();
            _completed = stats.Overview.CompletedContents;
            _totalContents = stats.Overview.TotalContents;
            _completionPercent = stats.Overview.CompletionPercent;

            var results = await Api.ResultsAsync();
            _attempts = results.Count;
            if (results.Count > 0)
            {
                _averageScore = (int)Math.Round(results.Average(r => r.ScorePercent));
                _bestScore = results.Max(r => r.ScorePercent);
            }
        }
        catch { }
    }

    private async Task Logout()
    {
        try { await Api.LogoutAsync(); } catch { }
        try { await Store.RemoveAsync(); } catch { }
        Auth.Clear();
    }
}
