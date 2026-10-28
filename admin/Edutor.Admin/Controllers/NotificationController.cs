using FluentValidation;
using Edutor.Core.Modules.Notification.Abstractions;
using Edutor.Core.Modules.Notification.Requests;
using Edutor.Core.Modules.Students.Abstractions;
using Edutor.Core.Modules.Students.Requests;
using Microsoft.AspNetCore.Mvc;
using Edutor.Admin.Infrastructure;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class NotificationController(
    INotificationService service,
    IStudentManagementService students,
    IValidator<CreateNotificationRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/notification/list.js", "~/js/pages/notification/create.js" };
        ViewData["Students"] = await students.ListAsync(new StudentListRequest { PerPage = 500 }, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] NotificationListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(new { }); // istifadə olunmur (list-də göstərilir)

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateNotificationRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, null, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

}
