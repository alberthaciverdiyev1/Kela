using LanguageLMS.Core.Modules.Tenant.Abstractions;
using LanguageLMS.Core.Modules.Tenant.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

[Route("[controller]")]
public sealed class TenantController(ITenantService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Tenantlar";
        ViewData["Scripts"] = new[] { "~/js/pages/tenant/list.js", "~/js/pages/tenant/create.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await service.ListAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request, CancellationToken ct)
        => Json(await service.CreateAsync(request, ct));
}
