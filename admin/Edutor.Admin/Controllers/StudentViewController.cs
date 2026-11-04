using System.Security.Claims;
using Edutor.Core.Modules.Students.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class StudentViewController(
    IStudentManagementService students,
    IConfiguration config) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        ViewData["Scripts"] = new[] { "~/js/pages/student-view/index.js" };
        ViewData["AppBaseUrl"] = AppBaseUrl;
        ViewData["StudentEmail"] = email;
        ViewData["HasStudentAccount"] = await students.ExistsByEmailAsync(email, ct);
        return View();
    }

    /// <summary>Şagird tətbiqinin ünvanı: konfiqurasiya, yoxdursa ilk CORS mənbəyi.</summary>
    private string AppBaseUrl
    {
        get
        {
            var configured = config["App:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(configured)) return configured.TrimEnd('/');

            var origin = config.GetSection("Cors:Origins").Get<string[]>()?
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            return origin?.TrimEnd('/') ?? "";
        }
    }
}
