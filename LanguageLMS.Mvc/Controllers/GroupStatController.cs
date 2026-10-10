using LanguageLMS.Core.Modules.Group.Abstractions;
using LanguageLMS.Core.Modules.Group.Requests;
using LanguageLMS.Core.Modules.Quiz.Abstractions;
using LanguageLMS.Core.Modules.Quiz.Requests;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

[Route("[controller]")]
public sealed class GroupStatController(
    IGroupQuizStatService stats,
    IGroupService groups) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Qrup statistikası";
        ViewData["Scripts"] = new[] { "~/js/pages/group-stat/index.js" };
        ViewData["Groups"] = await groups.ListAsync(new GroupListRequest { PerPage = 500 }, ct);
        return View();
    }

    [HttpGet("data")]
    public async Task<IActionResult> Data([FromQuery] GroupQuizStatRequest request, [FromQuery(Name = "group_id")] long groupId, CancellationToken ct)
        => Json(await stats.GetAsync(groupId, request, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] GroupQuizStatRequest request, [FromQuery(Name = "group_id")] long groupId, CancellationToken ct)
    {
        var bytes = await stats.ExportXlsxAsync(groupId, request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"group-{groupId}-quiz-stat.xlsx");
    }
}
