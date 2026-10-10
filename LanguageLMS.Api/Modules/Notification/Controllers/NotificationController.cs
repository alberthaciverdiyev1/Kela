using System.Security.Claims;
using LanguageLMS.Api.Common.Authorization;
using LanguageLMS.Api.Common.Controllers;
using LanguageLMS.Api.Common.Responses;
using LanguageLMS.Core.Common.Responses;
using LanguageLMS.Core.Modules.Auth;
using LanguageLMS.Core.Modules.Notification.Requests;
using LanguageLMS.Core.Modules.Notification.Responses;
using LanguageLMS.Core.Modules.Notification.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Api.Modules.Notification.Controllers;

[Authorize(Policy = AuthPolicies.Teacher)]
[HasPermission("notification.manage")]
[Route("api/[controller]")]
public sealed class NotificationController(INotificationService service) : ApiControllerBase
{
    private long? UserId => long.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationResponse>>>> List(
        [FromQuery] NotificationListRequest request, CancellationToken ct)
        => Success(await service.ListAsync(request, ct));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationResponse>>> Add(
        [FromForm] CreateNotificationRequest request, CancellationToken ct)
        => Success(await service.CreateAsync(request, UserId, ct), "Notification sent", StatusCodes.Status201Created);

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<string>>> Delete(long id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return SuccessMessage("Deleted");
    }
}
