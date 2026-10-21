using FluentValidation;
using Kela.Core.Common.Bulk;
using Kela.Core.Modules.Banner.Abstractions;
using Kela.Core.Modules.Banner.Requests;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;
using Kela.Core.Common.Requests;

namespace Kela.Admin.Controllers;

[Route("[controller]")]
public sealed class BannerController(
    IBannerService service,
    IValidator<BannerUpsertRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/banner/list.js", "~/js/pages/banner/create.js", "~/js/pages/banner/update.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] BannerListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    /// <summary>Admin bütün bannerləri idarə edir; teacher yalnız özünü.</summary>

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] BannerUpsertRequest request, CancellationToken ct)
    {
        request.UserId = OwnerScope;

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] BannerUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.UpdateAsync(id, request, OwnerScope, ct));
    }

    [HttpDelete("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPut("{id:long}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(long id, [FromBody] SetActiveRequest request, CancellationToken ct)
    {
        await service.SetActiveAsync(id, request.IsActive, OwnerScope, ct);
        return Json(new { ok = true });
    }

    [HttpPost("bulk")]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Bulk([FromBody] BulkRequest request, CancellationToken ct)
        => BulkAsync(request, service.BulkAsync, ct);

}
