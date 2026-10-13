using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Home;

public partial class Home
{
    private bool _loading = true;
    private bool _storyOpen;
    private int _storyIndex;
    private int _quizCount;
    private int _overallPercent;
    private int _completedContents;
    private int _totalContents;
    private List<BannerDto> _banners = [];
    private List<GroupDto> _studentGroups = [];
    private List<ProgressItemDto> _continue = [];

    private void OpenStory(int index)
    {
        _storyIndex = index;
        _storyOpen = true;
    }

    private void CloseStory() => _storyOpen = false;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var quizzes = await Api.QuizzesAsync();
            _banners = await Api.BannersAsync();
            _quizCount = quizzes.Count;
            // Qruplar — hər qrupun progressi backend-də hesablanıb gəlir
            _studentGroups = await Api.StudentGroupsAsync();

            // "Davam et" — növbəti materialı olan dərslər
            var progress = await Api.ProgressAsync();
            _continue = progress.Items
                .Where(i => i.NextContent is not null)
                .OrderByDescending(i => i.ProgressPercent)
                .Take(6)
                .ToList();

            // Ümumi progress də backend-də hesablanır
            var stats = await Api.StatisticsAsync();
            _overallPercent = stats.Overview.CompletionPercent;
            _completedContents = stats.Overview.CompletedContents;
            _totalContents = stats.Overview.TotalContents;
        }
        catch
        {
            // API əlçatmazsa boş qalır
        }

        _loading = false;
    }
}
