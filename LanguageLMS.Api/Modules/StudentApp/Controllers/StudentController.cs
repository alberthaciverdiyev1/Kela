using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Group.Responses;
using LanguageLMS.Core.Modules.Lesson.Responses;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using LanguageLMS.Core.Modules.Notification.Requests;
using LanguageLMS.Core.Modules.Notification.Responses;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.Notification.Services;
using LanguageLMS.Core.Modules.StudentApp.Services;
using LanguageLMS.Core.Modules.Subject.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.StudentApp.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentController(IStudentAppService service, INotificationService notifications) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupResponse>>>> Groups(CancellationToken ct)
        => Success(await service.ListGroupsAsync(StudentId, ct));

    [HttpGet("groups/{groupId:long}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> Subjects(long groupId, CancellationToken ct)
        => Success(await service.ListSubjectsAsync(StudentId, groupId, ct));

    [HttpGet("subjects/{subjectId:long}/lessons")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LessonResponse>>>> Lessons(long subjectId, CancellationToken ct)
        => Success(await service.ListLessonsAsync(StudentId, subjectId, ct));

    [HttpGet("lessons/{lessonId:long}/contents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LessonContentResponse>>>> Contents(long lessonId, CancellationToken ct)
        => Success(await service.ListContentsAsync(StudentId, lessonId, ct));

    [HttpGet("lessons/{lessonId:long}/content-count")]
    public async Task<ActionResult<ApiResponse<ContentCountResponse>>> ContentCount(long lessonId, CancellationToken ct)
        => Success(await service.GetContentCountAsync(StudentId, lessonId, ct));

    [HttpGet("quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> Quizzes(CancellationToken ct)
        => Success(await service.ListQuizzesAsync(StudentId, ct));

    [HttpGet("notifications")]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationResponse>>>> Notifications(
        [FromQuery] NotificationListRequest request, CancellationToken ct)
        => Success(await notifications.ListForStudentAsync(StudentId, request, ct));

    [HttpPost("notifications/token")]
    public async Task<ActionResult<ApiResponse<string>>> SaveToken(
        [FromBody] SaveNotificationTokenRequest request, CancellationToken ct)
    {
        await notifications.SaveTokenAsync(StudentId, request, ct);
        return SuccessMessage("Token saved");
    }

    [HttpDelete("notifications/token")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveToken([FromQuery] string token, CancellationToken ct)
    {
        await notifications.DeactivateTokenAsync(StudentId, token, ct);
        return SuccessMessage("Token removed");
    }
}
