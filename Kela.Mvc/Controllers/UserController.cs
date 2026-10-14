using FluentValidation;
using Kela.Core.Modules.User.Abstractions;
using Kela.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Mvc.Controllers;

[Route("[controller]")]
public sealed class UserController(
    IUserService service,
    IValidator<UserUpsertRequest> validator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "İstifadəçilər";
        ViewData["Scripts"] = new[] { "~/js/pages/user/list.js", "~/js/pages/user/create.js", "~/js/pages/user/update.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] UserListRequest request, CancellationToken ct)
        => Json(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] UserUpsertRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Errors(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] UserUpsertRequest request, CancellationToken ct)
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

    private static object Errors(FluentValidation.Results.ValidationResult result)
        => new
        {
            errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
        };
}
