using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Group.Requests;
using LanguageLMS.Core.Modules.Group.Responses;
using LanguageLMS.Core.Modules.Group.Services;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Group.Controllers;

[Route("api/[controller]")]
public sealed class GroupController(IGroupService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<GroupResponse>>>> List(
        [FromQuery] GroupListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Show(long id, CancellationToken ct)
        => Success(await service.GetAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Add(
        [FromForm] GroupUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, ct), "Created", StatusCodes.Status201Created);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<GroupResponse>>> Update(
        long id, [FromForm] GroupUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(id, request, ct), "Updated");

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }

    [HttpGet("{id:long}/students")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupMemberResponse>>>> ListStudents(
        long id, CancellationToken ct)
        => Success(await service.ListStudentsAsync(id, ct));

    [HttpPost("{id:long}/students")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupMemberResponse>>>> AssignStudents(
        long id, [FromBody] AssignStudentsRequest request, CancellationToken ct)
        => Success(await service.AssignStudentsAsync(id, request, ct), "Students assigned");

    [HttpDelete("{id:long}/students/{studentId:long}")]
    public async Task<ActionResult<ApiResponse<string>>> RemoveStudent(
        long id, long studentId, CancellationToken ct)
    {
        await service.RemoveStudentAsync(id, studentId, ct);
        return SuccessMessage("Student removed");
    }
}
