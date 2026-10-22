using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Subscription.Abstractions;
using Kela.Core.Modules.Subscription.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.Subscription.Controllers;

/// <summary>Cari tenant-ın abunəlik planı, limitləri və istifadəsi.</summary>
[Route("api/[controller]")]
[Authorize(Policy = AuthPolicies.Teacher)]
public sealed class SubscriptionController(ISubscriptionService subscription) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SubscriptionResponse>>> Get(CancellationToken ct)
        => Success(await subscription.GetCurrentAsync(ct));
}
