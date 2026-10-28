using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Progress.Requests;
using Edutor.Core.Modules.Progress.Responses;
using Edutor.Core.Modules.Progress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Modules.Progress.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class ProgressController(IProgressService service, IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProgressResponse>>> Get(CancellationToken ct)
        => Success(await service.GetProgressAsync(StudentId, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MarkProgressResponse>>> Mark(
        [FromBody] MarkProgressRequest request, CancellationToken ct)
        => Success(await service.MarkCompletedAsync(StudentId, request, ct), L["Api_ProgressSaved"]);
}
