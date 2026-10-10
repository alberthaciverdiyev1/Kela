using System.Globalization;
using LanguageLMS.App.Infrastructure.Http;

namespace LanguageLMS.App.Infrastructure.Api.Lessons;

public sealed class LessonApi(IApiClient apiClient) : ILessonApi
{
    public Task<ApiResult<PagedResponse<LessonListItem>>> ListAsync(LessonListQuery query, CancellationToken ct = default)
    {
        var parameters = new Dictionary<string, string?>
        {
            ["page"] = query.Page.ToString(CultureInfo.InvariantCulture),
            ["per_page"] = query.PerPage.ToString(CultureInfo.InvariantCulture),
            ["subject_id"] = query.SubjectId?.ToString(CultureInfo.InvariantCulture),
            ["is_active"] = query.IsActive?.ToString().ToLowerInvariant(),
            ["search"] = query.Search
        };

        var queryString = string.Join("&", parameters
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!)}"));

        return apiClient.GetAsync<PagedResponse<LessonListItem>>($"api/lesson?{queryString}", ct);
    }
}
