using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;

namespace Kela.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Catalog (groups / subjects / lessons / contents) ---
    public Task<List<GroupDto>> StudentGroupsAsync() => _api.GetAsync<List<GroupDto>>("/api/student/groups");
    public Task<GroupJoinInfoDto> JoinGroupInfoAsync(string token) => _api.GetAsync<GroupJoinInfoDto>($"/api/student/groups/join/{Uri.EscapeDataString(token)}");
    public Task<GroupJoinInfoDto> RequestJoinGroupAsync(string token) => _api.PostAsync<GroupJoinInfoDto>($"/api/student/groups/join/{Uri.EscapeDataString(token)}", new { });
    public Task<List<SubjectDto>> SubjectsAsync(long groupId) => _api.GetAsync<List<SubjectDto>>($"/api/student/groups/{groupId}/subjects");
    public Task<List<SubjectDto>> AllSubjectsAsync() => _api.GetAsync<List<SubjectDto>>("/api/student/subjects");
    public Task<SubjectDto> SubjectAsync(long subjectId) => _api.GetAsync<SubjectDto>($"/api/student/subjects/{subjectId}");
    public Task<List<LessonDto>> LessonsAsync(long subjectId) => _api.GetAsync<List<LessonDto>>($"/api/student/subjects/{subjectId}/lessons");
    public Task<LessonDto> LessonAsync(long lessonId) => _api.GetAsync<LessonDto>($"/api/student/lessons/{lessonId}");
    public Task<List<ContentItemDto>> ContentsAsync(long lessonId) => _api.GetAsync<List<ContentItemDto>>($"/api/student/lessons/{lessonId}/contents");
}
