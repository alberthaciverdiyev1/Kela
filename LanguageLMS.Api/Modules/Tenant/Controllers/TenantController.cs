using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Tenant.Requests;
using LanguageLMS.Core.Modules.Tenant.Responses;
using LanguageLMS.Core.Modules.Tenant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Tenant.Controllers;

[Route("api/[controller]")]
public sealed class TenantController(ITenantService service) : ApiControllerBase
{
    // Bootstrap: ilk tenant-ı yaratmaq üçün açıqdır (sonra super-admin ilə məhdudlaşdırılacaq)
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<TenantResponse>>> Create(
        [FromBody] CreateTenantRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [Authorize(Policy = AuthPolicies.Admin)]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantResponse>>>> List(CancellationToken ct)
        => Success(await service.ListAsync(ct));
}
