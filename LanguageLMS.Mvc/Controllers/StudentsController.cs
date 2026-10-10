using LanguageLMS.Core.Modules.Students.Abstractions;
using LanguageLMS.Core.Modules.Students.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

[Route("[controller]")]
public sealed class StudentsController(IStudentManagementService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Şagirdlər";
        ViewData["Scripts"] = new[] { "~/js/pages/students/list.js", "~/js/pages/students/generate.js", "~/js/pages/students/devices.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] StudentListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Status(long id, [FromBody] SetStudentStatusRequest request, CancellationToken ct)
        => Json(await service.SetStatusAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost("generate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate([FromBody] GenerateStudentsRequest request, CancellationToken ct)
        => Json(await service.GenerateAsync(request, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] StudentListRequest request, CancellationToken ct)
    {
        var bytes = await service.ExportXlsxAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"students-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
    }

    [HttpGet("{id:long}/devices")]
    public async Task<IActionResult> Devices(long id, CancellationToken ct)
        => Json(await service.ListDevicesAsync(id, ct));

    [HttpDelete("{id:long}/devices")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbindDevice(long id, CancellationToken ct)
    {
        await service.UnbindDeviceAsync(id, ct);
        return Json(new { ok = true });
    }
}
