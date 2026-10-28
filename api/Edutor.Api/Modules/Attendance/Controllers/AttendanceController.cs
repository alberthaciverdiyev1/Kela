using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Modules.Attendance.Responses;
using Edutor.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Api.Modules.Attendance.Controllers;

[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class AttendanceController(IAttendanceService service) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Şagirdin öz yoxlama xülasəsi (profil səhifəsi üçün).</summary>
    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<StudentAttendanceResponse>>> My(CancellationToken ct)
        => Success(await service.GetStudentSummaryAsync(StudentId, ct: ct));

    /// <summary>Şagirdin bir qrup üzrə ay təqvimi.</summary>
    [HttpGet("my/month")]
    public async Task<ActionResult<ApiResponse<StudentAttendanceMonthResponse>>> MyMonth(
        [FromQuery(Name = "group_id")] long groupId,
        [FromQuery] int year,
        [FromQuery] int month,
        CancellationToken ct)
        => Success(await service.GetStudentMonthAsync(StudentId, groupId, year, month, ct));
}
