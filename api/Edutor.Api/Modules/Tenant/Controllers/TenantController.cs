using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Modules.Tenant.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Api.Modules.Tenant.Controllers;

[Route("api/[controller]")]
public sealed class TenantController(ITenantService service) : ApiControllerBase
{
    /// <summary>Login səhifəsi üçün aktiv tenant-lar (public).</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantResponse>>>> List(CancellationToken ct)
        => Success(await service.ListPublicAsync(ct));
}
