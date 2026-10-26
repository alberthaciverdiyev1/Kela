using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Kela.Core.Modules.User.Abstractions;
using Kela.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class SubjectController(
    ISubjectService service,
    IUserService users,
    IValidator<SubjectUpsertRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/subject/list.js", "~/js/pages/subject/create.js", "~/js/pages/subject/update.js" };

        // Tək müəllim varsa "Bütün müəllimlər görsün" seçimi mənasızdır.
        var staff = await users.ListAsync(new UserListRequest { PerPage = 1 }, ct);
        ViewData["MultipleTeachers"] = staff.Total > 1;

        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] SubjectListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin())
        {
            // Teacher yalnız öz fənnlərini və adminin yaratdıqlarını görür.
            request.IsDeleted = 0;
            request.UserId = User.CurrentUserId();
        }
        return Json(await service.ListAsync(request, ct));
    }

    // Fənnin dərsləri ayrıca səhifə deyil: /lesson səhifəsi artıq fənnə görə
    // filtrlənir, ona görə köhnə ünvanları oraya yönləndiririk.
    [HttpGet("{id:long}")]
    public IActionResult Details(long id)
        => Redirect($"/lesson?subject_id={id}");

    // Redaktə modalı üçün JSON
    [HttpGet("{id:long}/edit")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, OwnerScope, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] SubjectUpsertRequest request, CancellationToken ct)
    {
        // Teacher yaratdığı fənnin sahibi olur.
        if (!User.IsAdmin()) request.UserId = User.CurrentUserId();

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] SubjectUpsertRequest request, CancellationToken ct)
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

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
        => BulkAsync(request, service.BulkAsync, ct);

}
