using System.Security.Claims;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.SystemNotification.Abstractions;
using LanguageLMS.Core.Modules.SystemNotification.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.SystemNotification.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[Route("api/[controller]")]
public sealed class SystemNotificationController(ISystemNotificationService service) : ApiControllerBase
{
    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SystemNotificationResponse>>>> List(CancellationToken ct)
        => Success(await service.ListAsync(UserId, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SystemNotificationResponse>>> Create(
        [FromBody] CreateSystemNotificationRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request.RecipientId, request.Title, request.Body, ct), "Created", StatusCodes.Status201Created);

    [HttpPost("{id:long}/read")]
    public async Task<ActionResult<ApiResponse<string>>> Read(long id, CancellationToken ct)
    {
        await service.MarkReadAsync(UserId, id, ct);
        return SuccessMessage("Marked as read");
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<string>>> ReadAll(CancellationToken ct)
    {
        await service.MarkAllReadAsync(UserId, ct);
        return SuccessMessage("All marked as read");
    }
}

public class CreateSystemNotificationRequest
{
    public long RecipientId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Body { get; set; }
}
