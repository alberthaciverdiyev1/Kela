using System.Security.Claims;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Payments.Abstractions;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Common.Middleware;

/// <summary>Ödənişi gecikən şagirdin "tam blok" rejimində sistemə girişini dayandırır.</summary>
public sealed class PaymentAccessMiddleware(RequestDelegate next, IStringLocalizer<SharedResource> L)
{
    // Bu uclar blokdan azaddır: şagird borcunu, bildirişlərini və profil məlumatını görə bilsin.
    private static readonly PathString[] Exempt =
    [
        "/api/auth",
        "/api/setting",
        "/api/student/payments",
        "/api/student/notifications"
    ];

    public async Task InvokeAsync(HttpContext context, IPaymentAccessService access)
    {
        if (!ShouldCheck(context)
            || !long.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var studentId))
        {
            await next(context);
            return;
        }

        var state = await access.GetAsync(studentId, context.RequestAborted);
        if (!state.BlockAll)
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(
            ApiResponse.Error(L["Payment_AccessBlocked"], StatusCodes.Status403Forbidden,
                new { code = "payment_required" }));
    }

    private static bool ShouldCheck(HttpContext context) =>
        context.User.Identity?.IsAuthenticated == true
        && context.User.IsInRole(AuthRoles.Student)
        && context.Request.Path.StartsWithSegments("/api")
        && !Exempt.Any(context.Request.Path.StartsWithSegments);
}
