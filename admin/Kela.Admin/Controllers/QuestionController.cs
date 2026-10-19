using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Modules.Question.Abstractions;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class QuestionController(
    IQuestionService service,
    ISubjectService subjects,
    IValidator<QuestionUpsertRequest> validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/question/list.js", "~/js/pages/question/create.js", "~/js/pages/question/update.js" };
        ViewData["Subjects"] = await subjects.ListAsync(new SubjectListRequest { PerPage = 500 }, ct);
        return View();
    }

    private long? CurrentUserId() =>
        long.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] QuestionListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            // Teacher yalnız öz qeydlərini görür.
            request.IsDeleted = 0;
            request.UserId = CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] QuestionUpsertRequest request, CancellationToken ct)
    {
        // Teacher yaratdığı qeydin sahibi olur (özü görə bilsin).
        if (!User.IsAdmin()) request.UserId = CurrentUserId();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] QuestionUpsertRequest request, CancellationToken ct)
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

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await service.BulkAsync(request, ct);
        return Json(new { ok = true });
    }

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
