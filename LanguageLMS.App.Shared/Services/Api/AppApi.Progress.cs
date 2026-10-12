using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services;

public sealed partial class AppApi
{
    // --- Progress ---
    public Task<ProgressOverview> ProgressAsync() => _api.GetAsync<ProgressOverview>("/api/progress");
    public Task<Statistics> StatisticsAsync() => _api.GetAsync<Statistics>("/api/statistics");
    public Task MarkCompletedAsync(long contentId) => _api.PostAsync<object>("/api/progress", new { contentId });
}
