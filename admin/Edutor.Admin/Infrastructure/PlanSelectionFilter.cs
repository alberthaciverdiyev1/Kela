using Edutor.Core.Modules.Subscription.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Edutor.Admin.Infrastructure;

/// <summary>
/// Plan seçməmiş tenant admini panelin hər hansı səhifəsindən plan seçiminə
/// yönləndirir. Superadmin, müəllim və anonim istifadəçilərə tətbiq olunmur.
/// </summary>
public sealed class PlanSelectionFilter(ISubscriptionService subscriptions) : IAsyncActionFilter
{
    private static readonly string[] Skipped =
        ["/plan-choose", "/login", "/register", "/home/error", "/health", "/_content"];

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        var path = request.Path.Value ?? string.Empty;
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true
            && user.IsInRole(UserTypes.Admin)
            && user.CurrentTenantId() is long tenantId and > 0
            && !Skipped.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            && await subscriptions.NeedsPlanSelectionAsync(tenantId, context.HttpContext.RequestAborted))
        {
            context.Result = new RedirectToActionResult("Index", "PlanChoose", null);
            return;
        }

        await next();
    }
}
