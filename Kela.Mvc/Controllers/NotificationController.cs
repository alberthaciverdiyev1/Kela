using FluentValidation;
using Kela.Core.Modules.Notification.Abstractions;
using Kela.Core.Modules.Notification.Requests;
using Kela.Core.Modules.Students.Abstractions;
using Kela.Core.Modules.Students.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class NotificationController(
    INotificationService service,
    IStudentManagementService students,
    IValidator<CreateNotificationRequest> validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Bildirişlər";
        ViewData["Scripts"] = new[] { "~/js/pages/notification/list.js", "~/js/pages/notification/create.js" };
        ViewData["Students"] = await students.ListAsync(new StudentListRequest { PerPage = 500 }, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] NotificationListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(new { }); // istifadə olunmur (list-də göstərilir)

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreateNotificationRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, null, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
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
