using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Payments.Abstractions;
using Kela.Core.Modules.Payments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public sealed class PaymentController(IStudentPaymentService payments) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Ödənişlər";
        ViewData["Scripts"] = new[] { "~/js/pages/payment/index.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(string? status, string? search, CancellationToken ct)
        => Json(await payments.ListAsync(status, search, ct));

    [HttpGet("{id:long}/history")]
    public async Task<IActionResult> History(long id, CancellationToken ct)
        => Json(await payments.HistoryAsync(id, ct));

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
