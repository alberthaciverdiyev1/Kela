using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Tenant.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.Tenant.Controllers;

[Route("api/[controller]")]
public sealed class TenantController(ITenantService service) : ApiControllerBase
{
    /// <summary>Login səhifəsi üçün aktiv tenant-lar (public).</summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TenantResponse>>>> List(CancellationToken ct)
        => Success(await service.ListPublicAsync(ct));
}
