using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Core.Common.Sorting;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Lesson.Requests;
using LanguageLMS.Core.Modules.Lesson.Responses;
using LanguageLMS.Core.Modules.Lesson.Services;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Lesson.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("lesson.manage")]
[Route("api/[controller]")]
public sealed class LessonController(ILessonService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<LessonResponse>>>> List(
        [FromQuery] LessonListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> Add(
        [FromForm] LessonUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<LessonResponse>>> Update(
        long id, [FromForm] LessonUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<string>>> Bulk([FromBody] BulkRequest request, CancellationToken ct)
    {
        await service.BulkAsync(request, ct);
        return SuccessMessage("Bulk action applied");
    }

    [HttpPost("sort")]
    public async Task<ActionResult<ApiResponse<string>>> Sort([FromBody] SortRequest request, CancellationToken ct)
    {
        await service.ReorderAsync(request, ct);
        return SuccessMessage("Order updated");
    }

    [HttpGet("{id:long}/settings")]
    public async Task<ActionResult<ApiResponse<LessonSettingsResponse>>> GetSettings(long id, CancellationToken ct)
        => Success(await service.GetSettingsAsync(id, ct));

    [HttpPut("{id:long}/settings")]
    public async Task<ActionResult<ApiResponse<LessonSettingsResponse>>> SetSettings(
        long id, [FromBody] SetLessonSettingsRequest request, CancellationToken ct)
        => Success(await service.SetSettingsAsync(id, request, ct), "Settings updated");
}
