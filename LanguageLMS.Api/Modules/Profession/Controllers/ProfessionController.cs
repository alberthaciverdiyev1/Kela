using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Profession.Requests;
using LanguageLMS.Core.Modules.Profession.Responses;
using LanguageLMS.Core.Modules.Profession.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Profession.Controllers;

public sealed class ProfessionController(IProfessionService service) : ApiControllerBase
{
    [HttpGet("api/profession")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProfessionResponse>>>> List(
        [FromQuery] ProfessionListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("api/profession/{id:long}")]
    public async Task<ActionResult<ApiResponse<ProfessionResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost("api/profession")]
    public async Task<ActionResult<ApiResponse<ProfessionResponse>>> Add(
        [FromForm] ProfessionUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("api/profession/{id:long}")]
    public async Task<ActionResult<ApiResponse<ProfessionResponse>>> Update(
        long id, [FromForm] ProfessionUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("api/profession/{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }
}
