using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Common.Localization;
using Kela.Core.Modules.Auth;
using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Quiz.Responses;
using Kela.Core.Modules.Quiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Kela.Api.Modules.Quiz.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentQuizController(IStudentQuizService service, IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<StartQuizResponse>>> Start(
        [FromBody] StartQuizRequest request, CancellationToken ct)
        => Success(await service.StartAsync(StudentId, request, ct), L["Api_QuizStarted"]);

    [HttpPost("complete")]
    public async Task<ActionResult<ApiResponse<CompleteQuizResponse>>> Complete(
        [FromBody] CompleteQuizRequest request, CancellationToken ct)
        => Success(await service.CompleteAsync(StudentId, request, ct), L["Api_QuizCompleted"]);

    [HttpGet("results")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResultSummary>>>> Results(
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
        => Success(await service.ListResultsAsync(StudentId, from, to, ct));

    [HttpGet("results/{attemptId:long}")]
    public async Task<ActionResult<ApiResponse<QuizResultDetail>>> Result(long attemptId, CancellationToken ct)
        => Success(await service.GetResultAsync(StudentId, attemptId, ct));
}
