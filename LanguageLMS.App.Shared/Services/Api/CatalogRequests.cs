using LanguageLMS.App.Shared.Models;

namespace LanguageLMS.App.Shared.Services.Api;

public sealed partial class AppApi
{
    // --- Catalog (groups / subjects / lessons / contents) ---
    public Task<List<Group>> GroupsAsync() => _api.GetAsync<List<Group>>("/api/student/groups");
    public Task<List<GroupResponse>> StudentGroupsAsync() => _api.GetAsync<List<GroupResponse>>("/api/student/groups");
    public Task<List<Subject>> SubjectsAsync(long groupId) => _api.GetAsync<List<Subject>>($"/api/student/groups/{groupId}/subjects");
    public Task<List<Lesson>> LessonsAsync(long subjectId) => _api.GetAsync<List<Lesson>>($"/api/student/subjects/{subjectId}/lessons");
    public Task<List<ContentItem>> ContentsAsync(long lessonId) => _api.GetAsync<List<ContentItem>>($"/api/student/lessons/{lessonId}/contents");
}
