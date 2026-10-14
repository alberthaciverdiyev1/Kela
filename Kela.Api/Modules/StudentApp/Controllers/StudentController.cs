using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Lesson.Responses;
using Kela.Core.Modules.LessonContent.Responses;
using Kela.Core.Modules.Notification.Requests;
using Kela.Core.Modules.Notification.Responses;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.Notification.Services;
using Kela.Core.Modules.StudentApp.Services;
using Kela.Core.Modules.StudentApp.Responses;
using Kela.Core.Modules.Subject.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.StudentApp.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentController(IStudentAppService service, INotificationService notifications) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentGroupResponse>>>> Groups(CancellationToken ct)
        => Success(await service.ListGroupsAsync(StudentId, ct));

    [HttpGet("groups/{groupId:long}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> Subjects(long groupId, CancellationToken ct)
        => Success(await service.ListSubjectsAsync(StudentId, groupId, ct));

    [HttpGet("subjects/{subjectId:long}")]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Subject(long subjectId, CancellationToken ct)
        => Success(await service.GetSubjectAsync(StudentId, subjectId, ct));

    [HttpGet("subjects/{subjectId:long}/lessons")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LessonResponse>>>> Lessons(long subjectId, CancellationToken ct)
        => Success(await service.ListLessonsAsync(StudentId, subjectId, ct));

    [HttpGet("lessons/{lessonId:long}")]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> Lesson(long lessonId, CancellationToken ct)
        => Success(await service.GetLessonAsync(StudentId, lessonId, ct));

    [HttpGet("lessons/{lessonId:long}/contents")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<LessonContentResponse>>>> Contents(long lessonId, CancellationToken ct)
        => Success(await service.ListContentsAsync(StudentId, lessonId, ct));

    [HttpGet("lessons/{lessonId:long}/content-count")]
    public async Task<ActionResult<ApiResponse<ContentCountResponse>>> ContentCount(long lessonId, CancellationToken ct)
        => Success(await service.GetContentCountAsync(StudentId, lessonId, ct));

    [HttpGet("quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> Quizzes(CancellationToken ct)
        => Success(await service.ListQuizzesAsync(StudentId, ct));

    [HttpGet("lessons/{lessonId:long}/quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> LessonQuizzes(long lessonId, CancellationToken ct)
        => Success(await service.ListLessonQuizzesAsync(StudentId, lessonId, ct));

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
