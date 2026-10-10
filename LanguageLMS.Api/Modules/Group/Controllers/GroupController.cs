using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Group.Requests;
using LanguageLMS.Core.Modules.Group.Responses;
using LanguageLMS.Core.Modules.Group.Services;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.Quiz.Services;
using LanguageLMS.Core.Modules.Subject.Responses;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Group.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("group.manage")]
[Route("api/[controller]")]
public sealed class GroupController(IGroupService service, IGroupQuizStatService statService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<GroupResponse>>>> List(
        [FromQuery] GroupListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Add(
        [FromForm] GroupUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Update(
        long id, [FromForm] GroupUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }

    [HttpGet("{id:long}/students")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupMemberResponse>>>> ListStudents(
        long id, CancellationToken ct)
        => Success(await service.ListStudentsAsync(id, ct));

    [HttpPost("{id:long}/students")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupMemberResponse>>>> AssignStudents(
        long id, [FromBody] AssignStudentsRequest request, CancellationToken ct)
        => Success(await service.AssignStudentsAsync(id, request, ct), "Students assigned");

    [HttpDelete("{id:long}/students/{studentId:long}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveStudent(
        long id, long studentId, CancellationToken ct)
    {
        await service.RemoveStudentAsync(id, studentId, ct);
        return SuccessMessage("Student removed");
    }

    [HttpGet("{id:long}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> ListSubjects(
        long id, CancellationToken ct)
        => Success(await service.ListSubjectsAsync(id, ct));

    [HttpPost("{id:long}/subjects")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SubjectResponse>>>> AssignSubjects(
        long id, [FromBody] AssignSubjectsRequest request, CancellationToken ct)
        => Success(await service.AssignSubjectsAsync(id, request, ct), "Subjects assigned");

    [HttpDelete("{id:long}/subjects/{subjectId:long}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveSubject(
        long id, long subjectId, CancellationToken ct)
    {
        await service.RemoveSubjectAsync(id, subjectId, ct);
        return SuccessMessage("Subject removed");
    }

    [HttpGet("{id:long}/quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> ListQuizzes(long id, CancellationToken ct)
        => Success(await service.ListQuizzesAsync(id, ct));

    [HttpGet("{id:long}/quiz-stat")]
    public async Task<ActionResult<ApiResponse<GroupQuizStatResponse>>> QuizStat(
        long id, [FromQuery] GroupQuizStatRequest request, CancellationToken ct)
        => Success(await statService.GetAsync(id, request, ct));

    [HttpPost("{id:long}/quizzes")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResponse>>>> AssignQuizzes(
        long id, [FromBody] AssignQuizzesRequest request, CancellationToken ct)
        => Success(await service.AssignQuizzesAsync(id, request, ct), "Quizzes assigned");

    [HttpDelete("{id:long}/quizzes/{quizId:long}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveQuiz(long id, long quizId, CancellationToken ct)
    {
        await service.RemoveQuizAsync(id, quizId, ct);
        return SuccessMessage("Quiz removed");
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<string>>> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        await service.BulkAsync(request, ct);
        return SuccessMessage("Bulk action applied");
    }

    [HttpGet("{id:long}/quiz-stat/export")]
    public async Task<IActionResult> QuizStatExport(long id, [FromQuery] GroupQuizStatRequest request, CancellationToken ct)
    {
        var bytes = await statService.ExportCsvAsync(id, request, ct);
        return File(bytes, "text/csv", $"group-{id}-quiz-stat.csv");
    }

    [HttpGet("{id:long}/subject-logs")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AssignmentLogResponse>>>> SubjectLogs(long id, CancellationToken ct)
        => Success(await service.ListSubjectLogsAsync(id, ct));
}
