using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Quiz.Requests;
using LanguageLMS.Core.Modules.Quiz.Responses;
using LanguageLMS.Core.Modules.Quiz.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Quiz.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentQuizController(IStudentQuizService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("start")]
    public async Task<ActionResult<ApiResponse<StartQuizResponse>>> Start(
        [FromBody] StartQuizRequest request, CancellationToken ct)
        => Success(await service.StartAsync(StudentId, request, ct), "Quiz started");

    [HttpPost("complete")]
    public async Task<ActionResult<ApiResponse<CompleteQuizResponse>>> Complete(
        [FromBody] CompleteQuizRequest request, CancellationToken ct)
        => Success(await service.CompleteAsync(StudentId, request, ct), "Quiz completed");

    [HttpGet("results")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<QuizResultSummary>>>> Results(CancellationToken ct)
        => Success(await service.ListResultsAsync(StudentId, ct));

    [HttpGet("results/{attemptId:long}")]
    public async Task<ActionResult<ApiResponse<QuizResultDetail>>> Result(long attemptId, CancellationToken ct)
        => Success(await service.GetResultAsync(StudentId, attemptId, ct));
}
