using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Students.Abstractions;
using LanguageLMS.Core.Modules.Students.Requests;
using LanguageLMS.Core.Modules.Students.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Students.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[Route("api/[controller]")]
public sealed class StudentsController(IStudentManagementService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<StudentListItemResponse>>>> List(
        [FromQuery] StudentListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<StudentListItemResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost("{id:long}/status")]
    public async Task<ActionResult<ApiResponse<StudentListItemResponse>>> Status(
        long id, [FromBody] SetStudentStatusRequest request, CancellationToken ct)
        => Success(await service.SetStatusAsync(id, request, ct), "Status updated");

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }

    [HttpPost("{id:long}/restore")]
    public async Task<ActionResult<ApiResponse<string>>> Restore(long id, CancellationToken ct)
    {
        await service.RestoreAsync(id, ct);
        return SuccessMessage("Restored");
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<string>>> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        await service.BulkAsync(request, ct);
        return SuccessMessage("Bulk action applied");
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] StudentListRequest request, CancellationToken ct)
    {
        var bytes = await service.ExportCsvAsync(request, ct);
        return File(bytes, "text/csv", $"students-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
    }
}
