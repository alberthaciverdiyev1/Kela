using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Core.Common.Bulk;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Subject.Requests;
using LanguageLMS.Core.Modules.Subject.Responses;
using LanguageLMS.Core.Modules.Subject.Services;
using LanguageLMS.Core.Modules.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Subject.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("subject.manage")]
[Route("api/[controller]")]
public sealed class SubjectController(ISubjectService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<SubjectResponse>>>> List(
        [FromQuery] SubjectListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Add(
        [FromForm] SubjectUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<SubjectResponse>>> Update(
        long id, [FromForm] SubjectUpsertRequest request, CancellationToken ct)
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
