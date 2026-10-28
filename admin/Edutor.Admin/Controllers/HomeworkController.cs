using FluentValidation;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Group.Abstractions;
using Edutor.Core.Modules.Group.Requests;
using Edutor.Core.Modules.Homework.Abstractions;
using Edutor.Core.Modules.Homework.Requests;
using Edutor.Core.Modules.Quiz.Abstractions;
using Edutor.Core.Modules.Quiz.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Ev tapşırıqları: müəllim yaradır, qruplara yayımlayır və qiymətləndirir.</summary>
[Route("[controller]")]
public sealed class HomeworkController(
    IHomeworkService service,
    IGroupService groups,
    IQuizService quizzes,
    IValidator<HomeworkUpsertRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery(Name = "group_id")] long? groupId, CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/homework/list.js",
            "~/js/pages/homework/create.js",
            "~/js/pages/homework/update.js"
        };

        ViewData["Groups"] = await LoadGroupsAsync(ct);
        ViewData["SelectedGroupId"] = groupId?.ToString() ?? "";
        ViewData["Quizzes"] = await quizzes.ListAsync(new QuizListRequest
        {
            PerPage = 500,
            UserId = User.IsAdmin() ? null : User.CurrentUserId()
        }, ct);

        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] HomeworkListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, OwnerScope, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, OwnerScope, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] HomeworkUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, OwnerScope, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] HomeworkUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.UpdateAsync(id, request, OwnerScope, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("{id:long}/publish")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(long id, CancellationToken ct)
        => Json(await service.PublishAsync(id, OwnerScope, ct));

    [HttpGet("{id:long}/manage")]
    public async Task<IActionResult> Manage(long id, CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/homework/manage-page.js" };

        var homework = await service.GetAsync(id, OwnerScope, ct);

        ViewData["Title"] = homework.Title;
        ViewData["Homework"] = homework;
        return View();
    }

    [HttpGet("{id:long}/assignments")]
    public async Task<IActionResult> Assignments(
        long id, [FromQuery] HomeworkAssignmentListRequest request, CancellationToken ct)
        => Json(await service.ListAssignmentsAsync(id, request, OwnerScope, ct));

    [HttpPut("{id:long}/assignments/{studentId:long}/grade")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Grade(
        long id, long studentId, [FromBody] GradeHomeworkRequest request, CancellationToken ct)
        => Json(await service.GradeAsync(id, studentId, request, OwnerScope, User.CurrentUserId(), ct));

    private Task<Edutor.Core.Common.Responses.PagedResponse<Edutor.Core.Modules.Group.Responses.GroupResponse>> LoadGroupsAsync(CancellationToken ct)
        => groups.ListAsync(new GroupListRequest
        {
            PerPage = 500,
            UserId = User.IsAdmin() ? null : User.CurrentUserId()
        }, ct);
}
