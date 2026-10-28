using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Quiz.Requests;
using Edutor.Core.Modules.Quiz.Responses;
using Edutor.Core.Modules.Quiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Modules.Quiz.Controllers;

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
