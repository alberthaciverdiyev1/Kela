using System.Security.Claims;
using Kela.Core.Modules.User.Abstractions;
using Kela.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>İstifadəçinin (superadmin/admin/teacher) öz profilini idarə etməsi.</summary>
[Authorize]
[Route("[controller]")]
public sealed class ProfileController(IUserService users) : Controller
{
    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Scripts"] = new[] { "~/js/pages/profile/index.js" };
        return View(await users.GetAsync(UserId, ct));
    }

    [HttpPut]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] UserProfileRequest request, CancellationToken ct)
    {
        var user = await users.UpdateProfileAsync(UserId, request, ct);
        await RefreshSignInAsync(user.Name, user.Surname, user.Email);
        return Json(user);
    }

    [HttpPut("password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromForm] UserChangePasswordRequest request, CancellationToken ct)
    {
        await users.ChangePasswordAsync(UserId, request, ct);
        return Json(new { ok = true });
    }

    /// <summary>Ad/email dəyişdikdə cookie claim-lərini yeniləyir (topbar dərhal yenilənsin).</summary>
    private async Task RefreshSignInAsync(string? name, string? surname, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ""),
            new(ClaimTypes.Name, $"{name} {surname}".Trim()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, User.FindFirst(ClaimTypes.Role)?.Value ?? "")
        };

        var tenantId = User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantId))
        {
            claims.Add(new Claim("tenant_id", tenantId));
            claims.Add(new Claim("tenant_name", User.FindFirst("tenant_name")?.Value ?? ""));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
