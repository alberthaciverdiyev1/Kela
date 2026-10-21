using FluentValidation;
using Kela.Core.Common.Exceptions;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.User.Abstractions;
using Kela.Core.Modules.User.Entities;
using Kela.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kela.Admin.Infrastructure;

namespace Kela.Admin.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class UserController(
    IUserService service,
    IValidator<UserUpsertRequest> validator) : AdminControllerBase
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Scripts"] = new[] { "~/js/pages/user/list.js", "~/js/pages/user/create.js", "~/js/pages/user/update.js" };
        return View();
    }

    [HttpGet("list")]
    public async Task<IActionResult> List([FromQuery] UserListRequest request, CancellationToken ct)
    {
        if (!User.IsAdmin()) request.IsDeleted = 0;
        return Json(await service.ListAsync(request, ct));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
        => Json(await service.GetAsync(id, ct));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] UserUpsertRequest request, CancellationToken ct)
    {
        EnsureCanAssignType(request.Type);
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid) return BadRequest(Invalid(result));

        return Json(await service.CreateAsync(request, ct));
    }

    [HttpPut("{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(long id, [FromForm] UserUpsertRequest request, CancellationToken ct)
    {
        EnsureCanAssignType(request.Type);
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

    private void EnsureCanAssignType(string type)
    {
        if (type == UserTypes.SuperAdmin && !User.IsInRole(AuthRoles.SuperAdmin))
        {
            throw new ForbiddenException("Yalnız superadmin başqa superadmin təyin edə bilər");
        }
    }

}
