using FluentValidation;
using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.Subscription.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Abunəlik planları — yalnız superadmin. Qiymət və limitlər buradan dəyişir.</summary>
[Route("[controller]")]
[Authorize(Policy = AuthPolicies.SuperAdmin)]
public sealed class PlanController(ISubscriptionService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/plan/list.js",
            "~/js/pages/plan/form.js"
        };

        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await service.ListPlansAsync(ct: ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetPlanAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] PlanRequest request, CancellationToken ct)
        => Json(await service.CreatePlanAsync(request, ct));

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] PlanRequest request, CancellationToken ct)
        => Json(await service.UpdatePlanAsync(id, request, ct));

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeletePlanAsync(id, ct);
        return Json(new { ok = true });
    }
}
