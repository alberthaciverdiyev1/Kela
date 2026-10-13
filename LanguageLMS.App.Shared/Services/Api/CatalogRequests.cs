using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;

namespace LanguageLMS.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Catalog (groups / subjects / lessons / contents) ---
    public Task<List<GroupDto>> StudentGroupsAsync() => _api.GetAsync<List<GroupDto>>("/api/student/groups");
    public Task<List<SubjectDto>> SubjectsAsync(long groupId) => _api.GetAsync<List<SubjectDto>>($"/api/student/groups/{groupId}/subjects");
    public Task<SubjectDto> SubjectAsync(long subjectId) => _api.GetAsync<SubjectDto>($"/api/student/subjects/{subjectId}");
    public Task<List<LessonDto>> LessonsAsync(long subjectId) => _api.GetAsync<List<LessonDto>>($"/api/student/subjects/{subjectId}/lessons");
    public Task<LessonDto> LessonAsync(long lessonId) => _api.GetAsync<LessonDto>($"/api/student/lessons/{lessonId}");
    public Task<List<ContentItemDto>> ContentsAsync(long lessonId) => _api.GetAsync<List<ContentItemDto>>($"/api/student/lessons/{lessonId}/contents");
}
