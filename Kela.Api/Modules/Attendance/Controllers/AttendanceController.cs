using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Core.Modules.Attendance.Responses;
using Kela.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Api.Modules.Attendance.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class AttendanceController(IAttendanceService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Şagirdin öz yoxlama xülasəsi (profil səhifəsi üçün).</summary>
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<StudentAttendanceResponse>>> My(CancellationToken ct)
        => Success(await service.GetStudentSummaryAsync(StudentId, ct: ct));
}
