using Kela.Core.Modules.Group.Abstractions;
using Kela.Core.Modules.Group.Requests;
using Kela.Core.Modules.Quiz.Abstractions;
using Kela.Core.Modules.Quiz.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class GroupStatController(
    IGroupQuizStatService stats,
    IGroupService groups) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/group-stat/index.js" };
        // Teacher yalnız öz qruplarını görür.
        ViewData["Groups"] = await groups.ListAsync(
            new GroupListRequest { PerPage = 500, UserId = User.IsAdmin() ? null : CurrentUserId() }, ct);
        return View();
    }

    private long? CurrentUserId() =>
        long.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    [HttpGet("data")]
    public async Task<IActionResult> Data([FromQuery] GroupQuizStatRequest request, [FromQuery(Name = "group_id")] long groupId, CancellationToken ct)
        => await CanReadAsync(groupId, ct) ? Json(await stats.GetAsync(groupId, request, ct)) : Forbid();

    /// <summary>Teacher yalnız öz qrupunun statistikasını görə bilər.</summary>
    private async Task<bool> CanReadAsync(long groupId, CancellationToken ct)
    {
        if (User.IsAdmin()) return true;

        try
        {
            var group = await groups.GetAsync(groupId, ct: ct);
            return group.UserId == User.CurrentUserId();
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
