using System.Diagnostics;
using Kela.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public class HomeController : Controller
{
    [HttpGet("[action]")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
