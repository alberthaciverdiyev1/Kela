using Edutor.Core.Modules.Tenant.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Edutor.Admin.Infrastructure;

/// <summary>
/// Fərdi müəllim tenant-ı tək nəfərlikdir — istifadəçi idarəetməsi ona bağlıdır, çünki
/// orada başqa müəllim yarada bilərdi. Superadmin istisnadır.
/// </summary>
public sealed class IndividualTeacherGuardFilter(ITenantService tenants) : IAsyncActionFilter
{
    private const string IndividualTeacherAccount = "teacher";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        var user = http.User;

        if (user.Identity?.IsAuthenticated != true
            || user.IsInRole(UserTypes.SuperAdmin)
            || !http.Request.Path.StartsWithSegments("/user"))
        {
            await next();
            return;
        }

        var accountType = await tenants.GetCurrentAccountTypeAsync(http.RequestAborted);
        if (accountType != IndividualTeacherAccount)
        {
            await next();
            return;
        }

        if (http.Request.Path.Equals("/user", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToActionResult("Index", "Dashboard", null);
            return;
        }

        context.Result = new ObjectResult(new { error = "Fərdi müəllim hesabı istifadəçi idarəetməsinə giriş edə bilməz." })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
