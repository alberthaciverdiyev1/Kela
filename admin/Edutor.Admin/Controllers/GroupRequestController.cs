using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Group.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Bütün qruplar üzrə linklə qoşulma müraciətləri — ayrı səhifə.</summary>
[Route("[controller]")]
public sealed class GroupRequestController(IGroupService service) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/group-request/index.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(string? status, CancellationToken ct)
        => Json(await service.ListJoinRequestsInboxAsync(OwnerScope, status, ct));

    [HttpPost("{groupId:long}/{id:long}/approve")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(long groupId, long id, CancellationToken ct)
    {
        await service.DecideJoinRequestAsync(groupId, id, true, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("{groupId:long}/{id:long}/reject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(long groupId, long id, CancellationToken ct)
    {
        await service.DecideJoinRequestAsync(groupId, id, false, OwnerScope, ct);
        return Json(new { ok = true });
    }
}
