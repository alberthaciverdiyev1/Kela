using Edutor.Core.Common.Exceptions;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Payments.Abstractions;
using Edutor.Core.Modules.Payments.Requests;
using Edutor.Core.Modules.Subscription;
using Edutor.Core.Modules.Subscription.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

/// <summary>Tələbəyə ödəniş planı təyin edərkən seçilən hazır plan şablonları.</summary>
[Route("[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public sealed class PaymentPlanController(IStudentPaymentService payments, ISubscriptionService subscriptions)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var enabled = await subscriptions.HasFeatureAsync(SubscriptionFeatures.PaymentTemplates, ct);

        ViewData["Title"] = "Plan şablonları";
        ViewData["TemplatesEnabled"] = enabled;

        if (enabled)
        {
            ViewData["Scripts"] = new[] { "~/js/pages/payment-plan/index.js" };
        }

        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        await EnsureEnabledAsync(ct);
        return Json(await payments.ListTemplatesAsync(ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] SavePaymentPlanTemplateRequest request, CancellationToken ct)
    {
        await EnsureEnabledAsync(ct);
        await payments.SaveTemplateAsync(request, ct);
        return Json(new { ok = true });
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await EnsureEnabledAsync(ct);
        await payments.DeleteTemplateAsync(id, ct);
        return Json(new { ok = true });
    }

    private async Task EnsureEnabledAsync(CancellationToken ct)
    {
        if (!await subscriptions.HasFeatureAsync(SubscriptionFeatures.PaymentTemplates, ct))
        {
            throw new ForbiddenException("Plan şablonları cari abunəlik planında yoxdur. Planı yüksəldin.");
        }
    }
}
