using System.Globalization;
using System.Text;
using Kela.Admin.Infrastructure;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Payments.Abstractions;
using Kela.Core.Modules.Payments.Requests;
using Kela.Core.Modules.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Admin.Controllers;

/// <summary>
/// Kassa: tələbələrdən daxil olan ödənişlər, aylıq gəlir və proqnoz.
/// Admin həmişə, müəllim isə yalnız fərdi müəllim (kurs olmayan) tenant-da görür.
/// </summary>
[Route("[controller]")]
[Authorize(Policy = AuthPolicies.Teacher)]
public sealed class KassaController(
    IKassaService kassa,
    IStudentPaymentService payments,
    ITenantService tenants) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        ViewData["Scripts"] = new[] { "~/js/pages/kassa/index.js" };
        return View(await kassa.GetOverviewAsync(ct));
    }

    [HttpGet("students")]
    public async Task<IActionResult> Students(CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        var list = await payments.ListAsync(null, null, 1, 500, ct);

        return Json(list.Data.Select(s => new
        {
            id = s.StudentId,
            name = s.FullName,
            amount = s.Amount,
            currency = s.Currency,
            nextDueOn = s.NextDueOn,
            status = s.Status
        }));
    }

    [HttpGet("plans")]
    public async Task<IActionResult> Plans(CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        return Json(await payments.ListTemplatesAsync(ct));
    }

    [HttpGet("payments")]
    public async Task<IActionResult> PaymentRange(
        [FromQuery(Name = "from")] DateOnly? from,
        [FromQuery(Name = "to")] DateOnly? to,
        CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        return Json(await kassa.ListPaymentsAsync(from, to, ct));
    }

    [HttpPost("plan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePlan([FromBody] SavePaymentPlanRequest request, CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        try
        {
            await payments.SavePlanAsync(request, ct);
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

    [HttpPost("payments")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        try
        {
            await payments.RecordPaymentAsync(request, ct);
            return Json(new { ok = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("payments/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePayment(long id, CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        await payments.DeletePaymentAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        if (!await CanAccessAsync(ct)) return Forbid();

        var rows = await kassa.ListPaymentsAsync(null, null, ct);

        var csv = new StringBuilder("Tarix,Tələbə,Məbləğ,Valyuta,Qeyd\n");
        foreach (var row in rows)
        {
            csv.Append(CultureInfo.InvariantCulture, $"{row.PaidOn:yyyy-MM-dd},")
               .Append(CultureInfo.InvariantCulture, $"\"{row.StudentName.Replace("\"", "\"\"")}\",")
               .Append(CultureInfo.InvariantCulture, $"{row.Amount:0.00},{row.Currency},")
               .Append(CultureInfo.InvariantCulture, $"\"{row.Note?.Replace("\"", "\"\"")}\"\n");
        }

        // BOM Excel-də Azərbaycan hərflərinin düzgün açılması üçün.
        return File(Encoding.UTF8.GetBytes("﻿" + csv), "text/csv", $"kassa-{DateTime.Today:yyyyMMdd}.csv");
    }

    private async Task<bool> CanAccessAsync(CancellationToken ct)
        => User.IsAdmin() || await tenants.GetCurrentAccountTypeAsync(ct) == "teacher";
}
