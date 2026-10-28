using FluentValidation;
using Edutor.Core.Common.Exceptions;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Students.Abstractions;
using Edutor.Core.Modules.Students.Requests;
using Edutor.Core.Modules.User.Abstractions;
using Edutor.Core.Modules.User.Entities;
using Edutor.Core.Modules.User.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Edutor.Admin.Infrastructure;

namespace Edutor.Admin.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
[Route("[controller]")]
public sealed class UserController(
    IUserService service,
    IStudentManagementService students,
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
        // Şagird ayrı cədvəldə saxlanılır — yaradılması student servisinə ötürülür.
        if (request.Type == AuthRoles.Student)
        {
            try
            {
                var created = await students.CreateAsync(new CreateStudentRequest
                {
                    FullName = $"{request.Name} {request.Surname}".Trim(),
                    Email = request.Email,
                    Phone = request.PhoneNumber,
                    Password = request.Password
                }, ct);

                return Json(new
                {
                    created.Id,
                    created.FullName,
                    created.Email,
                    created.Password,
                    Type = AuthRoles.Student
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

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
        // Öz hesabını silmək kilidlənmə riskidir — heç bir rol üçün icazə verilmir.
        if (id == User.CurrentUserId())
        {
            throw new ForbiddenException("Öz hesabınızı silə bilməzsiniz");
        }

        var target = await service.GetAsync(id, ct);

        // Admin hesablarını yalnız superadmin silə bilər.
        if (!User.IsInRole(AuthRoles.SuperAdmin) && target.Type is UserTypes.Admin or UserTypes.SuperAdmin)
        {
            throw new ForbiddenException("Admin hesabını yalnız superadmin silə bilər");
        }

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
