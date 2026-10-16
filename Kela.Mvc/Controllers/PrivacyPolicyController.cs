using Kela.Core.Modules.Auth;
using Kela.Core.Modules.PrivacyPolicy.Abstractions;
using Kela.Core.Modules.PrivacyPolicy.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
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
