using Kela.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleParent)]
public sealed class ParentController : Controller
{
    public IActionResult Dashboard() => View();

    public IActionResult Children()
    {
        ViewData["TitleKey"] = "nav.myChildren";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }
}
