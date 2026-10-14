using Kela.Core.Modules.Permission.Abstractions;
using Kela.Core.Modules.Permission.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class PermissionController(IPermissionService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "İcazələr";
        ViewData["Scripts"] = new[] { "~/js/pages/permission/list.js", "~/js/pages/permission/create.js", "~/js/pages/permission/role.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await service.ListAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreatePermissionRequest request, CancellationToken ct)
        => Json(await service.CreateAsync(request, ct));

    [HttpPost("role")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRole([FromBody] SetRolePermissionsRequest request, CancellationToken ct)
    {
        await service.SetRolePermissionsAsync(request, ct);
        return Json(new { ok = true });
    }
}
