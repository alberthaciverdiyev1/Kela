using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Progress.Requests;
using LanguageLMS.Core.Modules.Progress.Responses;
using LanguageLMS.Core.Modules.Progress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Progress.Controllers;

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
