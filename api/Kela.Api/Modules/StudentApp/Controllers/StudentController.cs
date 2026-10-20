using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Common.Responses;
using Kela.Core.Common.Localization;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Lesson.Responses;
using Kela.Core.Modules.LessonContent.Responses;
using Kela.Core.Modules.Notification.Requests;
using Kela.Core.Modules.Notification.Responses;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.Notification.Services;
using Kela.Core.Modules.Payments.Abstractions;
using Kela.Core.Modules.Payments.Responses;
using Kela.Core.Modules.StudentApp.Services;
using Kela.Core.Modules.StudentApp.Responses;
using Kela.Core.Modules.Subject.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Kela.Api.Modules.StudentApp.Controllers;

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
