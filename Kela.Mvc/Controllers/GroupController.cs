using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Modules.Group.Abstractions;
using Kela.Core.Modules.Group.Requests;
using Kela.Core.Modules.Group.Responses;
using Kela.Core.Modules.Quiz.Abstractions;
using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Students.Abstractions;
using Kela.Core.Modules.Students.Requests;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Kela.Core.Modules.User.Abstractions;
using Kela.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class GroupController(
    IGroupService service,
    IUserService users,
    IStudentManagementService students,
    ISubjectService subjects,
    IQuizService quizzes,
    IValidator<GroupUpsertRequest> validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/group/list.js", "~/js/pages/group/create.js",
            "~/js/pages/group/update.js", "~/js/pages/group/manage.js"
        };
        ViewData["Teachers"] = await users.ListAsync(new UserListRequest { PerPage = 200 }, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] GroupListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] GroupUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] GroupUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    // --- İdarəetmə: şagird / fənn / imtahan ---

    [HttpGet("options")]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        var studentList = await students.ListAsync(new StudentListRequest { PerPage = 500 }, ct);
        var subjectList = await subjects.ListAsync(new SubjectListRequest { PerPage = 500 }, ct);
        var quizList = await quizzes.ListAsync(new QuizListRequest { PerPage = 500 }, ct);

        return Json(new GroupOptionsResponse(
            studentList.Data.Select(s => new OptionItem(s.Id, s.FullName)).ToList(),
            subjectList.Data.Select(s => new OptionItem(s.Id, s.Name)).ToList(),
            quizList.Data.Select(q => new OptionItem(q.Id, q.Title)).ToList()));
    }

    [HttpGet("{id:long}/students")]
    public async Task<IActionResult> GroupStudents(long id, CancellationToken ct)
        => Json(await service.ListStudentsAsync(id, ct));

    [HttpPost("{id:long}/students")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStudents(long id, [FromBody] AssignStudentsRequest request, CancellationToken ct)
        => Json(await service.AssignStudentsAsync(id, request, ct));

    [HttpDelete("{id:long}/students/{studentId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(long id, long studentId, CancellationToken ct)
    {
        await service.RemoveStudentAsync(id, studentId, ct);
        return Json(new { ok = true });
    }

    [HttpGet("{id:long}/subjects")]
    public async Task<IActionResult> GroupSubjects(long id, CancellationToken ct)
        => Json(await service.ListSubjectsAsync(id, ct));

    [HttpPost("{id:long}/subjects")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSubjects(long id, [FromBody] AssignSubjectsRequest request, CancellationToken ct)
        => Json(await service.AssignSubjectsAsync(id, request, ct));

    [HttpDelete("{id:long}/subjects/{subjectId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSubject(long id, long subjectId, CancellationToken ct)
    {
        await service.RemoveSubjectAsync(id, subjectId, ct);
        return Json(new { ok = true });
    }

    [HttpGet("{id:long}/quizzes")]
    public async Task<IActionResult> GroupQuizzes(long id, CancellationToken ct)
        => Json(await service.ListQuizzesAsync(id, ct));

    [HttpPost("{id:long}/quizzes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignQuizzes(long id, [FromBody] AssignQuizzesRequest request, CancellationToken ct)
        => Json(await service.AssignQuizzesAsync(id, request, ct));

    [HttpDelete("{id:long}/quizzes/{quizId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuiz(long id, long quizId, CancellationToken ct)
    {
        await service.RemoveQuizAsync(id, quizId, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        await service.BulkAsync(request, ct);
        return Json(new { ok = true });
    }

    // Fənn atama/çıxarma audit loqları
    [HttpGet("{id:long}/subject-logs")]
    public async Task<IActionResult> SubjectLogs(long id, CancellationToken ct)
        => Json(await service.ListSubjectLogsAsync(id, ct));

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
