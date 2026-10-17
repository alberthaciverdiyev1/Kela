using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Feedback.Abstractions;
using Kela.Core.Modules.Feedback.Entities;
using Kela.Core.Modules.Feedback.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>Geri bildiriş söhbətləri — yalnız superadmin.</summary>
[Authorize(Policy = AuthPolicies.SuperAdmin)]
[Route("[controller]")]
public sealed class FeedbackController(IFeedbackService service) : Controller
{
    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] FeedbackListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("unread")]
    public async Task<IActionResult> Unread(CancellationToken ct)
        => Json(new { count = await service.UnreadCountAsync(ct) });

    [HttpPost("{id:long}/read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Read(long id, CancellationToken ct)
    {
        await service.MarkReadAsync(id, FeedbackRoles.Admin, ct);
        return Json(new { ok = true });
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }
}
