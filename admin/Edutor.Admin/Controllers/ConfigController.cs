using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Group.Abstractions;
using Edutor.Core.Modules.Group.Requests;
using Edutor.Core.Modules.TenantConfig.Abstractions;
using Edutor.Core.Modules.TenantConfig.Requests;
using Edutor.Core.Modules.TenantConfig.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class ConfigController(
    ITenantConfigService service,
    IGroupService groups,
    TenantConfigUpsertRequestValidator validator) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/config/index.js" };
        return View();
    }

    [HttpGet("current")]
    public async Task<IActionResult> Current(CancellationToken ct)
        => Json(await service.GetAsync(ct));

    [HttpGet("groups")]
    public async Task<IActionResult> Groups(CancellationToken ct)
    {
        var result = await groups.ListAsync(new GroupListRequest { PerPage = 500 }, ct);
        return Json(result.Data.Select(g => new { id = g.Id, name = g.Name }));
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] TenantConfigUpsertRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid) return BadRequest(Invalid(validation));

        return Json(await service.UpdateAsync(request, ct));
    }
}
