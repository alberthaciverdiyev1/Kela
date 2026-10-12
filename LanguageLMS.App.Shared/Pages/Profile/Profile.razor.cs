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

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _groups = (await Api.StudentGroupsAsync()).Count;
            _quizzes = (await Api.QuizzesAsync()).Count;

            var stats = await Api.StatisticsAsync();
            _completed = stats.Overview.CompletedContents;
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
