using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Payments.Abstractions;
using Edutor.Core.Modules.Students.Abstractions;
using Edutor.Core.Modules.Payments.Requests;
using Edutor.Core.Modules.Subscription;
using Edutor.Core.Modules.Subscription.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public sealed class PaymentController(
    IStudentPaymentService payments,
    IStudentManagementService students,
    ISubscriptionService subscriptions) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["Title"] = "Ödənişlər";
        ViewData["Scripts"] = new[] { "~/js/pages/payment/index.js" };
        ViewData["TemplatesEnabled"] = await subscriptions.HasFeatureAsync(SubscriptionFeatures.PaymentTemplates, ct);
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        string? status, string? search, int page = 1, int perPage = 20, CancellationToken ct = default)
        => Json(await payments.ListAsync(status, search, page, perPage, ct));

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Json(await payments.GetSummaryAsync(ct));

    [HttpDelete("payments/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(long id, CancellationToken ct)
    {
        await payments.DeletePaymentAsync(id, ct);
        return Json(new { ok = true });
    }

    /// <summary>Şagirdin ödəniş tarixçəsi — ayrıca səhifə.</summary>
    [HttpGet("{id:long}/history")]
    public async Task<IActionResult> History(long id, CancellationToken ct)
    {
        var student = await students.GetAsync(id, ct);
        ViewData["Title"] = student.FullName;
        ViewData["Scripts"] = new[] { "~/js/pages/payment/history.js" };
        ViewData["Student"] = student;
        ViewData["History"] = await payments.HistoryAsync(id, ct);
        return View();
    }

    [HttpPost("plan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePlan([FromBody] SavePaymentPlanRequest request, CancellationToken ct)
    {
        try { await payments.SavePlanAsync(request, ct); return Json(new { ok = true }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpPost("record")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        try { await payments.RecordPaymentAsync(request, ct); return Json(new { ok = true }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
