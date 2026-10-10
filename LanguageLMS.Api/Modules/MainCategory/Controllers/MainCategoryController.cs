using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.MainCategory.Requests;
using LanguageLMS.Core.Modules.MainCategory.Responses;
using LanguageLMS.Core.Modules.MainCategory.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.MainCategory.Controllers;

[Route("api/[controller]")]
public sealed class MainCategoryController(IMainCategoryService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<MainCategoryResponse>>>> List(
        [FromQuery] MainCategoryListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<MainCategoryResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<MainCategoryResponse>>> Add(
        [FromForm] MainCategoryUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<MainCategoryResponse>>> Update(
        long id, [FromForm] MainCategoryUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }
}
