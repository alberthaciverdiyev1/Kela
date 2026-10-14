using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Progress.Requests;
using Kela.Core.Modules.Progress.Responses;
using Kela.Core.Modules.Progress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.Progress.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class ProgressController(IProgressService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProgressResponse>>> Get(CancellationToken ct)
        => Success(await service.GetProgressAsync(StudentId, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MarkProgressResponse>>> Mark(
        [FromBody] MarkProgressRequest request, CancellationToken ct)
        => Success(await service.MarkCompletedAsync(StudentId, request, ct), "Progress saved");
}
