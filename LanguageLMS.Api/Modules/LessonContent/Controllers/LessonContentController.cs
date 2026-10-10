using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.LessonContent.Requests;
using LanguageLMS.Core.Modules.LessonContent.Responses;
using LanguageLMS.Core.Modules.LessonContent.Services;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.LessonContent.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("lesson.manage")]
[Route("api/[controller]")]
public sealed class LessonContentController(ILessonContentService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<LessonContentResponse>>>> List(
        [FromQuery] LessonContentListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<LessonContentResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost("upload-url")]
    public async Task<ActionResult<ApiResponse<UploadUrlResponse>>> UploadUrl(
        [FromBody] LessonContentUploadUrlRequest request, CancellationToken ct)
        => Success(await service.CreateUploadUrlAsync(request, ct), "Upload URL created");

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LessonContentResponse>>> Add(
        [FromForm] LessonContentCreateRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<LessonContentResponse>>> Update(
        long id, [FromForm] LessonContentUpdateRequest request, CancellationToken ct)
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
}
