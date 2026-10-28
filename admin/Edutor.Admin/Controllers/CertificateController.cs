using FluentValidation;
using Edutor.Core.Common.Bulk;
using Edutor.Core.Modules.Certificate;
using Edutor.Core.Modules.Certificate.Abstractions;
using Edutor.Core.Modules.Certificate.Requests;
using Microsoft.AspNetCore.Mvc;
using Edutor.Admin.Infrastructure;

namespace Edutor.Admin.Controllers;

[Route("[controller]")]
public sealed class CertificateController(
    ICertificateService service,
    IValidator<CertificateUpsertRequest> validator) : AdminControllerBase
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
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] CertificateUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

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
    public Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
        => BulkAsync(request, service.BulkAsync, ct);

}
