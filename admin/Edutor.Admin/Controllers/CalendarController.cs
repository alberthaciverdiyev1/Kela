using Edutor.Admin.Infrastructure;
using Edutor.Core.Modules.Calendar.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class CalendarController(ICalendarService service) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/calendar/index.js" };
        return View();
    }

    [HttpGet("month")]
    public async Task<IActionResult> Month(int year, int month, CancellationToken ct)
        => Json(await service.GetMonthAsync(
            User.CurrentUserId() ?? 0, User.IsAdmin(), year, month, ct));
}
