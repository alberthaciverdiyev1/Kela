using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Modules.Certificate;
using Kela.Core.Modules.Certificate.Abstractions;
using Kela.Core.Modules.Certificate.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Mvc.Infrastructure;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class CertificateController(
    ICertificateService service,
    IValidator<CertificateUpsertRequest> validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Templates"] = CertificateTemplates.All;
        ViewData["Scripts"] = new[]
        {
            "~/js/pages/certificate/list.js",
            "~/js/pages/certificate/create.js",
            "~/js/pages/certificate/update.js"
        };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] CertificateListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CertificateUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] CertificateUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.UpdateAsync(id, request, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        // Bərpa yalnız adminin əməliyyatıdır — silinmişləri yalnız o görür.
        if (request.Action == BulkActions.Restore && !User.IsAdmin()) return Forbid();
        await service.BulkAsync(request, ct);
        return Json(new { ok = true });
    }

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
