using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Localization;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.StudentNote.Requests;
using Edutor.Core.Modules.StudentNote.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Modules.StudentNote.Controllers;

/// <summary>Şagird qeydləri: materiala bağlı və müstəqil qeydlər.</summary>
[Authorize(Policy = AuthPolicies.Student)]
[Route("api/[controller]")]
public sealed class StudentNoteController(IStudentNoteService service, IStringLocalizer<SharedResource> L) : ApiControllerBase
{
    private long StudentId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentNoteResponse>>>> List(
        [FromQuery(Name = "content_id")] long? contentId,
        [FromQuery(Name = "lesson_id")] long? lessonId,
        [FromQuery] string? scope,
        [FromQuery(Name = "only_free")] bool? onlyFree,
        CancellationToken ct)
        => Success(await service.ListAsync(StudentId, new StudentNoteListRequest
        {
            ContentId = contentId,
            LessonId = lessonId,
            Scope = scope,
            OnlyFree = onlyFree
        }, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StudentNoteResponse>>> Create(
        [FromBody] StudentNoteUpsertRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(StudentId, request, ct), L["Api_Created"]);

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<StudentNoteResponse>>> Update(
        long id, [FromBody] StudentNoteUpsertRequest request, CancellationToken ct)
        => Success(await service.UpdateAsync(StudentId, id, request, ct), L["Api_Updated"]);

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(StudentId, id, ct);
        return SuccessMessage(L["Api_Deleted"]);
    }
}
