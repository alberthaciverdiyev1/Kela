using Kela.App.Shared.Dtos.Homework;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    public Task<List<StudentHomeworkSummaryDto>> HomeworksAsync()
        => _api.GetAsync<List<StudentHomeworkSummaryDto>>("/api/student-homework");

    public Task<StudentHomeworkDetailDto> HomeworkAsync(long id)
        => _api.GetAsync<StudentHomeworkDetailDto>($"/api/student-homework/{id}");

    public Task<StudentHomeworkDetailDto> SubmitHomeworkAsync(
        long id, string? text, string? link, IReadOnlyList<HomeworkUploadFile> files)
        => _api.PostHomeworkSubmissionAsync<StudentHomeworkDetailDto>(
            $"/api/student-homework/{id}/submission", text, link, files);
}
