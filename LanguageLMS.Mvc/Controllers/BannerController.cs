using FluentValidation;
using LanguageLMS.Core.Modules.Banner.Abstractions;
using LanguageLMS.Core.Modules.Banner.Requests;
using LanguageLMS.Core.Modules.Banner.Services;
using LanguageLMS.Mvc.Models.Banner;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Mvc.Controllers;

public sealed class BannerController(
    IBannerService service,
    IValidator<BannerUpsertRequest> validator) : Controller
{
    public async Task<IActionResult> Index(BannerListRequest request, CancellationToken ct)
        => View(new BannerIndexViewModel
        {
            Search = request.Search,
            Page = await service.ListAsync(request, ct)
        });

    [HttpGet]
    public IActionResult Create() => View(new BannerFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BannerFormViewModel vm, CancellationToken ct)
    {
        var request = ToRequest(vm);
        if (!await ValidateAsync(request)) return View(vm);

        await service.CreateAsync(request, ct);
        TempData["Success"] = "Created";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(long id, CancellationToken ct)
    {
        var banner = await FindAsync(id, ct);
        if (banner is null) return NotFound();

        return View(new BannerFormViewModel
        {
            Id = banner.Id,
            Title = banner.Title,
            CurrentImageUrl = banner.Image
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, BannerFormViewModel vm, CancellationToken ct)
    {
        var request = ToRequest(vm);
        if (!await ValidateAsync(request))
        {
            vm.Id = id;
            return View(vm);
        }

        await service.UpdateAsync(id, request, ct);
        TempData["Success"] = "Updated";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(long id, CancellationToken ct)
    {
        var banner = await FindAsync(id, ct);
        return banner is null ? NotFound() : View(banner);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        TempData["Success"] = "Deleted";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Core.Modules.Banner.Responses.BannerResponse?> FindAsync(long id, CancellationToken ct)
    {
        try
        {
            return await service.GetAsync(id, ct);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    private static BannerUpsertRequest ToRequest(BannerFormViewModel vm) => new()
    {
        Title = vm.Title,
        Image = vm.Image,
        RemoveImage = vm.RemoveImage
    };

    private async Task<bool> ValidateAsync(BannerUpsertRequest request)
    {
        var result = await validator.ValidateAsync(request);
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return result.IsValid;
    }
}
