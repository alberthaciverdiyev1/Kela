using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Payments.Abstractions;
using Kela.Core.Modules.Students.Abstractions;
using Kela.Core.Modules.Payments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public sealed class PaymentController(IStudentPaymentService payments, IStudentManagementService students) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Ödənişlər";
        ViewData["Scripts"] = new[] { "~/js/pages/payment/index.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(
        string? status, string? search, int page = 1, int perPage = 20, CancellationToken ct = default)
        => Json(await payments.ListAsync(status, search, page, perPage, ct));

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
        => Json(await payments.GetSummaryAsync(ct));

    // --- Plan şablonları ---

    [HttpGet("templates")]
    public IActionResult Templates()
    {
        ViewData["Title"] = "Plan şablonları";
        ViewData["Scripts"] = new[] { "~/js/pages/payment/templates.js" };
        return View();
    }

    [HttpGet("templates/list")]
    public async Task<IActionResult> TemplateList(CancellationToken ct)
        => Json(await payments.ListTemplatesAsync(ct));

    [HttpPost("templates")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTemplate([FromBody] SavePaymentPlanTemplateRequest request, CancellationToken ct)
    {
        try
        {
            await payments.SaveTemplateAsync(request, ct);
            return Json(new { ok = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("templates/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTemplate(long id, CancellationToken ct)
    {
        await payments.DeleteTemplateAsync(id, ct);
        return Json(new { ok = true });
    }

    /// <summary>Şagirdin ödəniş tarixçəsi — ayrıca səhifə.</summary>
    [HttpGet("{id:long}/history")]
    public async Task<IActionResult> History(long id, CancellationToken ct)
    {
        var student = await students.GetAsync(id, ct);
        ViewData["Title"] = student.FullName;
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
