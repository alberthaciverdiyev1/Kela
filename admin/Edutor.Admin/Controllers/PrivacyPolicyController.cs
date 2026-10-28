using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.PrivacyPolicy.Abstractions;
using Edutor.Core.Modules.PrivacyPolicy.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class PrivacyPolicyController(IPrivacyPolicyService service) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/privacy-policy/list.js", "~/js/pages/privacy-policy/edit.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await service.ListAsync(ct));

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert([FromForm] UpsertPrivacyPolicyRequest request, CancellationToken ct)
        => Json(await service.UpsertAsync(request, ct));
}
