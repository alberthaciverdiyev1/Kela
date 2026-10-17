using Kela.Core.Modules.Nodes.Abstractions;
using Kela.Core.Modules.Nodes.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
[Authorize(Policy = Core.Modules.Auth.AuthPolicies.Admin)]
public sealed class NodeController(INodeService nodes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/node/list.js",
            "~/js/pages/node/codes.js",
            "~/js/pages/node/jobs.js"
        };

        ViewData["Summary"] = await nodes.SummaryAsync(ct);
        ViewData["Nodes"] = await nodes.ListAsync(new NodeListRequest { PerPage = 500 }, ct);

        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] NodeListRequest request, CancellationToken ct)
        => Json(await nodes.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await nodes.GetAsync(id, ct));

    /// <summary>Cihazı işə salır / söndürür (token də ləğv olunur).</summary>
    [HttpPut("{id:long}/enabled")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetEnabled(long id, [FromQuery] bool enabled, CancellationToken ct)
        => Json(await nodes.SetEnabledAsync(id, enabled, ct));

    [HttpDelete("{id:long}/token")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(long id, CancellationToken ct)
    {
        await nodes.RevokeTokenAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await nodes.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    // ── Qeydiyyat kodları ────────────────────────────────────────────────────

    [HttpGet("codes")]
    public async Task<IActionResult> Codes(CancellationToken ct)
        => Json(await nodes.ListCodesAsync(ct));

    [HttpPost("codes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCode([FromForm] CreateRegistrationCodeRequest request, CancellationToken ct)
        => Json(await nodes.CreateCodeAsync(request, ct));

    [HttpDelete("codes/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCode(long id, CancellationToken ct)
    {
        await nodes.DeleteCodeAsync(id, ct);
        return Json(new { ok = true });
    }

    // ── Tapşırıqlar ──────────────────────────────────────────────────────────

    [HttpGet("jobs")]
    public async Task<IActionResult> Jobs([FromQuery] VideoJobListRequest request, CancellationToken ct)
        => Json(await nodes.ListJobsAsync(request, ct));

    [HttpPost("jobs/{id:long}/requeue")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Requeue(long id, CancellationToken ct)
    {
        await nodes.RequeueJobAsync(id, ct);
        return Json(new { ok = true });
    }
}
