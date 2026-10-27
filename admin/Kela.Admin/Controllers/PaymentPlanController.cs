using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Payments.Abstractions;
using Kela.Core.Modules.Payments.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>Tələbəyə ödəniş planı təyin edərkən seçilən hazır plan şablonları.</summary>
[Route("[controller]")]
[Authorize(Policy = AuthPolicies.Admin)]
public sealed class PaymentPlanController(IStudentPaymentService payments) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Plan şablonları";
        ViewData["Scripts"] = new[] { "~/js/pages/payment-plan/index.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Json(await payments.ListTemplatesAsync(ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] SavePaymentPlanTemplateRequest request, CancellationToken ct)
    {
        try { await payments.SaveTemplateAsync(request, ct); return Json(new { ok = true }); }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await payments.DeleteTemplateAsync(id, ct);
        return Json(new { ok = true });
    }
}
