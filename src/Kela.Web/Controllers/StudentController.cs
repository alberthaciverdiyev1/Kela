using Kela.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Web.Controllers;

[Authorize(Roles = AppConstants.RoleStudent)]
public sealed class StudentController : Controller
{
    public IActionResult Dashboard() => View();

    public IActionResult Courses()
    {
        ViewData["TitleKey"] = "nav.myCourses";
        return View("~/Views/Shared/ComingSoon.cshtml");
    }
}
