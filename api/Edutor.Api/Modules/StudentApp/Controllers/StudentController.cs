using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Group.Responses;
using Edutor.Core.Modules.Lesson.Responses;
using Edutor.Core.Modules.LessonContent.Responses;
using Edutor.Core.Modules.Notification.Requests;
using Edutor.Core.Modules.Notification.Responses;
using Edutor.Core.Modules.Quiz.Responses;
using Edutor.Core.Modules.Notification.Services;
using Edutor.Core.Modules.Payments.Abstractions;
using Edutor.Core.Modules.Payments.Responses;
using Edutor.Core.Modules.StudentApp.Services;
using Edutor.Core.Modules.StudentApp.Responses;
using Edutor.Core.Modules.Subject.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Modules.StudentApp.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentController(IStudentAppService service, INotificationService notifications, IStudentPaymentService payments, IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("groups")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentGroupResponse>>>> Groups(CancellationToken ct)
        => Success(await service.ListGroupsAsync(StudentId, ct));

    [AllowAnonymous]
    [HttpGet("groups/join/{token}/public")]
    public async Task<ActionResult<ApiResponse<GroupJoinInfoResponse>>> PublicJoinInfo(string token, CancellationToken ct)
        => Success(await service.GetPublicGroupJoinInfoAsync(token, ct));

    [HttpGet("groups/join/{token}")]
    public async Task<ActionResult<ApiResponse<GroupJoinInfoResponse>>> JoinInfo(string token, CancellationToken ct)
        => Success(await service.GetGroupJoinInfoAsync(StudentId, token, ct));

    [HttpPost("groups/join/{token}")]
    public async Task<ActionResult<ApiResponse<GroupJoinInfoResponse>>> Join(string token, CancellationToken ct)
        => Success(await service.RequestJoinGroupAsync(StudentId, token, ct));

    [HttpGet("groups/{groupId:long}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> Subjects(long groupId, CancellationToken ct)
        => Success(await service.ListSubjectsAsync(StudentId, groupId, ct));

    [HttpGet("subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> AllSubjects(CancellationToken ct)
        => Success(await service.ListAllSubjectsAsync(StudentId, ct));

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
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> Quizzes(
        [FromQuery(Name = "subject_id")] long? subjectId,
        [FromQuery(Name = "lesson_id")] long? lessonId,
        [FromQuery] bool general,
        CancellationToken ct)
        => Success(await service.ListQuizzesAsync(StudentId, subjectId, lessonId, general, ct));

    [HttpGet("lessons/{lessonId:long}/quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> LessonQuizzes(long lessonId, CancellationToken ct)
        => Success(await service.ListLessonQuizzesAsync(StudentId, lessonId, ct));

    [HttpGet("payments")]
    public async Task<ActionResult<ApiResponse<StudentPaymentOverviewResponse>>> Payments(CancellationToken ct)
        => Success(await payments.GetStudentOverviewAsync(StudentId, ct));

    [HttpGet("notifications")]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationResponse>>>> Notifications(
        [FromQuery] NotificationListRequest request, CancellationToken ct)
        => Success(await notifications.ListForStudentAsync(StudentId, request, ct));

    [HttpPost("notifications/token")]
    public async Task<ActionResult<ApiResponse<string>>> SaveToken(
        [FromBody] SaveNotificationTokenRequest request, CancellationToken ct)
    {
        await notifications.SaveTokenAsync(StudentId, request, ct);
        return SuccessMessage(L["Api_TokenSaved"]);
    }

    [HttpDelete("notifications/token")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveToken([FromQuery] string token, CancellationToken ct)
    {
        await notifications.DeactivateTokenAsync(StudentId, token, ct);
        return SuccessMessage(L["Api_TokenRemoved"]);
    }
}
