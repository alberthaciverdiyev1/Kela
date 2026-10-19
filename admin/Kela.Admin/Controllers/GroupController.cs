using Kela.Core.Modules.Lesson.Requests;
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
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class GroupController(
    IGroupService service,
    IUserService users,
    Kela.Core.Modules.Lesson.Abstractions.ILessonService lessons,
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
            "~/js/pages/group/update.js"
        };
        ViewData["Teachers"] = await users.ListAsync(new UserListRequest { PerPage = 200 }, ct);
        return View();
    }

    [HttpGet("{id:long}/manage")]
    public async Task<IActionResult> Manage(long id, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/group/manage-page.js" };

        var owner = OwnerScope;
        var group = await service.GetAsync(id, owner, ct);

        // Teacher başqasının qrupunu URL ilə də aça bilməsin.
        if (!User.IsAdmin() && group.UserId != owner)
            return Forbid();

        ViewData["Title"] = group.Name;
        ViewData["Group"] = group;
        ViewData["Teachers"] = await users.ListAsync(new UserListRequest { PerPage = 200 }, ct);
        ViewData["SubjectCount"] = (await service.ListSubjectsAsync(id, owner, ct)).Count;
        ViewData["QuizCount"] = (await service.ListQuizzesAsync(id, owner, ct)).Count;
        return View();
    }

    private long? OwnerScope => User.IsAdmin() ? null : User.CurrentUserId();

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] GroupListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            // Teacher yalnız öz qruplarını görür; admin/superadmin hamısını.
            request.IsDeleted = 0;
            request.UserId = User.CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, OwnerScope, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] GroupUpsertRequest request, CancellationToken ct)
    {
        // Teacher yalnız özünə qrup yaradır — müəllim seçimi məcburi özüdür.
        if (!User.IsAdmin()) request.UserId = User.CurrentUserId();

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

        return Json(await service.UpdateAsync(id, request, OwnerScope, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, OwnerScope, ct);
        return Json(new { ok = true });
    }

    // --- İdarəetmə: şagird / fənn / imtahan ---

    [HttpGet("options")]
    public async Task<IActionResult> Options(CancellationToken ct)
    {
        var studentList = await students.ListAsync(new StudentListRequest { PerPage = 500 }, ct);
        var ownerId = User.IsAdmin() ? null : User.CurrentUserId();
        var subjectList = await subjects.ListAsync(new SubjectListRequest { PerPage = 500, UserId = ownerId }, ct);
        // Teacher üçün imtahan seçimi yalnız öz imtahanları ilə məhdudlaşır.
        var quizList = await quizzes.ListAsync(
            new QuizListRequest { PerPage = 500, UserId = ownerId }, ct);
        var lessonList = await lessons.ListAsync(new LessonListRequest { PerPage = 500, UserId = ownerId }, ct);

        // İmtahan → dərs → fənn: siyahıda imtahanları seçilmiş fənnlərə görə filtrləmək üçün.
        var lessonSubject = lessonList.Data.ToDictionary(l => l.Id, l => l.SubjectId);

        return Json(new GroupOptionsResponse(
            studentList.Data.Select(s => new OptionItem(s.Id, s.FullName)).ToList(),
            subjectList.Data.Select(s => new OptionItem(s.Id, s.Name)).ToList(),
            quizList.Data.Select(q => new OptionItem(
                q.Id,
                q.Title,
                q.LessonTitle,
                q.LessonId is long lessonId && lessonSubject.TryGetValue(lessonId, out var subjectId) ? subjectId : null)).ToList()));
    }

    [HttpGet("{id:long}/students")]
    public async Task<IActionResult> GroupStudents(long id, CancellationToken ct)
        => Json(await service.ListStudentsAsync(id, OwnerScope, ct));

    [HttpPost("{id:long}/students")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignStudents(long id, [FromBody] AssignStudentsRequest request, CancellationToken ct)
        => Json(await service.AssignStudentsAsync(id, request, OwnerScope, ct));

    [HttpDelete("{id:long}/students/{studentId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(long id, long studentId, CancellationToken ct)
    {
        await service.RemoveStudentAsync(id, studentId, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpGet("{id:long}/subjects")]
    public async Task<IActionResult> GroupSubjects(long id, CancellationToken ct)
        => Json(await service.ListSubjectsAsync(id, OwnerScope, ct));

    [HttpPost("{id:long}/subjects")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignSubjects(long id, [FromBody] AssignSubjectsRequest request, CancellationToken ct)
        => Json(await service.AssignSubjectsAsync(id, request, OwnerScope, ct));

    [HttpDelete("{id:long}/subjects/{subjectId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSubject(long id, long subjectId, CancellationToken ct)
    {
        await service.RemoveSubjectAsync(id, subjectId, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpGet("{id:long}/quizzes")]
    public async Task<IActionResult> GroupQuizzes(long id, CancellationToken ct)
        => Json(await service.ListQuizzesAsync(id, OwnerScope, ct));

    [HttpPost("{id:long}/quizzes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignQuizzes(long id, [FromBody] AssignQuizzesRequest request, CancellationToken ct)
        => Json(await service.AssignQuizzesAsync(id, request, OwnerScope, ct));

    [HttpDelete("{id:long}/quizzes/{quizId:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveQuiz(long id, long quizId, CancellationToken ct)
    {
        await service.RemoveQuizAsync(id, quizId, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await service.BulkAsync(request, OwnerScope, ct);
        return Json(new { ok = true });
    }

    // Fənn atama/çıxarma audit loqları
    [HttpGet("{id:long}/subject-logs")]
    public async Task<IActionResult> SubjectLogs(long id, CancellationToken ct)
        => Json(await service.ListSubjectLogsAsync(id, OwnerScope, ct));

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
