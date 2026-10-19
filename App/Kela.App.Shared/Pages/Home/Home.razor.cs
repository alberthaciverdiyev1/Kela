using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Home;

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
    private readonly HashSet<long> _seenStories = new();
    private const string SeenStoriesKey = "lms.story.seen";

    [Inject] private IJSRuntime JS { get; set; } = default!;
    private List<GroupDto> _studentGroups = [];
    private List<ProgressItemDto> _continue = [];

    private void OpenStory(int index)
    {
        _storyIndex = index;
        _storyOpen = true;
    }

    /// <summary>Baxılan story-lər yadda saxlanılır — yaşıl həlqə yalnız görülməmişdə qalır.</summary>
    private async Task MarkStorySeenAsync(long id)
    {
        if (!_seenStories.Add(id)) return;

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_seenStories.ToList());
            await JS.InvokeVoidAsync("localStorage.setItem", SeenStoriesKey, json);
        }
        catch { /* localStorage əlçatmaz ola bilər */ }
    }

    private async Task LoadSeenStoriesAsync()
    {
        try
        {
            var json = await JS.InvokeAsync<string?>("localStorage.getItem", SeenStoriesKey);
            if (string.IsNullOrWhiteSpace(json)) return;

            var ids = System.Text.Json.JsonSerializer.Deserialize<List<long>>(json);
            if (ids is not null)
            {
                foreach (var id in ids) _seenStories.Add(id);
            }
        }
        catch { /* səssiz */ }
    }

    private void CloseStory() => _storyOpen = false;

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await LoadSeenStoriesAsync();
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        try
        {
            var quizzes = await Api.QuizzesAsync();
            // Vaxtı keçmiş story-lər göstərilmir (API də filtr edir — burada əlavə qoruma).
            var today = DateOnly.FromDateTime(DateTime.Now);
            _banners = (await Api.BannersAsync())
                .Where(b => b.ActiveUntil is null || b.ActiveUntil.Value >= today)
                .ToList();
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
    }
}
