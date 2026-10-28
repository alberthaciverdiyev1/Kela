using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Homework.Requests;
using Edutor.Core.Modules.Homework.Responses;
using Edutor.Core.Modules.Homework.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Api.Modules.Homework.Controllers;

/// <summary>Tələbə tərəfi: ona təyin olunmuş ev tapşırıqları və təqdimat.</summary>
[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentHomeworkController(IStudentHomeworkService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentHomeworkSummaryResponse>>>> List(CancellationToken ct)
        => Success(await service.ListAsync(StudentId, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<StudentHomeworkDetailResponse>>> Get(long id, CancellationToken ct)
        => Success(await service.GetAsync(StudentId, id, ct));

    [HttpPost("{id:long}/submission")]
    [RequestSizeLimit(HomeworkFileRules.MaxRequestBytes)]
    public async Task<ActionResult<ApiResponse<StudentHomeworkDetailResponse>>> Submit(
        long id, [FromForm] SubmitHomeworkRequest request, CancellationToken ct)
        => Success(await service.SubmitAsync(StudentId, id, request, ct));
}
