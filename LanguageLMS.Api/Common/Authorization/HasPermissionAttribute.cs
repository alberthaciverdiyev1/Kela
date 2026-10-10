using System.Security.Claims;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Permission.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LanguageLMS.Api.Common.Authorization;

/// <summary>İncə icazə yoxlaması. admin həmişə keçir; rol üçün icazə təyin olunmayıbsa məhdudiyyət yox.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasPermissionAttribute(string permission) : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
        if (role is null)
        {
            context.Result = Denied();
            return;
        }

        var service = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
        if (!await service.HasAsync(role, permission, context.HttpContext.RequestAborted))
        {
            context.Result = Denied();
        }
    }

    private static ObjectResult Denied()
        => new(ApiResponse.Error("Permission denied", StatusCodes.Status403Forbidden))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
}
