using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Progress ---
    public Task<ProgressOverviewDto> ProgressAsync() => _api.GetAsync<ProgressOverviewDto>("/api/progress");
    public Task<StatisticsDto> StatisticsAsync() => _api.GetAsync<StatisticsDto>("/api/statistics");
    public Task MarkCompletedAsync(long contentId) => _api.PostAsync<object>("/api/progress", new { contentId });
}
