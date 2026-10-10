using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.PrivacyPolicy.Requests;
using LanguageLMS.Core.Modules.PrivacyPolicy.Responses;
using LanguageLMS.Core.Modules.PrivacyPolicy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.PrivacyPolicy.Controllers;

[Route("api/[controller]")]
public sealed class PrivacyPolicyController(IPrivacyPolicyService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PrivacyPolicyResponse>>>> List(CancellationToken ct)
        => Success(await service.ListAsync(ct));

    [HttpGet("{type}")]
    public async Task<ActionResult<ApiResponse<PrivacyPolicyResponse>>> Get(string type, CancellationToken ct)
        => Success(await service.GetAsync(type, ct));

    [Authorize(Policy = AuthPolicies.Teacher)]
    [HttpPut]
    public async Task<ActionResult<ApiResponse<PrivacyPolicyResponse>>> Upsert(
        [FromForm] UpsertPrivacyPolicyRequest request, CancellationToken ct)
        => Success(await service.UpsertAsync(request, ct), "Saved");
}
