using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Permission.Requests;
using LanguageLMS.Core.Modules.Permission.Responses;
using LanguageLMS.Core.Modules.Permission.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Permission.Controllers;

[Authorize(Policy = AuthPolicies.Admin)]
[Route("api/[controller]")]
public sealed class PermissionController(IPermissionService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PermissionResponse>>>> List(CancellationToken ct)
        => Success(await service.ListAsync(ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PermissionResponse>>> Add(
        [FromBody] CreatePermissionRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("role")]
    public async Task<ActionResult<ApiResponse<string>>> SetRole(
        [FromBody] SetRolePermissionsRequest request, CancellationToken ct)
    {
        await service.SetRolePermissionsAsync(request, ct);
        return SuccessMessage("Role permissions updated");
    }
}
