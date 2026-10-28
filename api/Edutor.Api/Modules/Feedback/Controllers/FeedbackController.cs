using System.Security.Claims;
using Edutor.Api.Common.Controllers;
using Edutor.Api.Common.Responses;
using Edutor.Api.Modules.Feedback.Hubs;
using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Auth;
using Edutor.Core.Modules.Feedback.Abstractions;
using Edutor.Core.Modules.Feedback.Entities;
using Edutor.Core.Modules.Feedback.Requests;
using Edutor.Core.Modules.Feedback.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Edutor.Api.Modules.Feedback.Controllers;

[Route("api/[controller]")]
public sealed class FeedbackController(IFeedbackService service, IHubContext<FeedbackHub> hub) : ApiControllerBase
{
    [HttpPost]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<FeedbackThreadResponse>>> Create(
        [FromForm] CreateFeedbackRequest request, CancellationToken ct)
    {
        FillSender(request);
        var thread = await service.CreateAsync(request, ct);

        await hub.Clients.Group(FeedbackHub.SuperAdminsGroup)
            .SendAsync("feedback:message", thread, ct);

        return Success(thread);
    }

    /// <summary>Şagird/müəllim öz söhbətlərini görür.</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<PagedResponse<FeedbackThreadResponse>>>> Mine(
        [FromQuery] FeedbackListRequest request, CancellationToken ct)
    {
        request.UserId = CurrentUserId();
        if (request.UserId is null) return Forbid();
        return Success(await service.ListAsync(request, ct));
    }

    /// <summary>Həm şagird, həm superadmin (admin panel) mesaj göndərir.</summary>
    [HttpPost("{id:long}/messages")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<FeedbackThreadResponse>>> Send(
        long id, [FromForm] AddFeedbackMessageRequest request, CancellationToken ct)
    {
        var isAdmin = User.IsInRole(AuthRoles.SuperAdmin);

        request.SenderRole = isAdmin ? FeedbackRoles.Admin : FeedbackRoles.User;
        request.SenderName = SenderName();

        // Şagird yalnız öz söhbətinə yazır; superadmin istənilən söhbətə.
        if (!isAdmin)
        {
            var owner = await service.OwnerIdAsync(id, ct);
            if (owner is null || owner != CurrentUserId()) return Forbid();
        }

        var thread = await service.AddMessageAsync(id, request, ct);

        if (isAdmin)
            await hub.Clients.Group(FeedbackHub.UserGroup(thread.UserId ?? 0))
                .SendAsync("feedback:message", thread, ct);
        else
            await hub.Clients.Group(FeedbackHub.SuperAdminsGroup)
                .SendAsync("feedback:message", thread, ct);

        return Success(thread);
    }

    [HttpPost("{id:long}/read")]
    public async Task<IActionResult> Read(long id, CancellationToken ct)
    {
        var role = User.IsInRole(AuthRoles.SuperAdmin) ? FeedbackRoles.Admin : FeedbackRoles.User;
        await service.MarkReadAsync(id, role, ct);
        return Ok(new { ok = true });
    }

    private void FillSender(CreateFeedbackRequest request)
    {
        request.UserId = CurrentUserId();
        request.UserName = SenderName();
        request.UserEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        request.Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
    }

    // JWT-də ad "full_name" claim-indədir (ClaimTypes.Name deyil).
    private string SenderName() =>
        User.FindFirst("full_name")?.Value
        ?? User.FindFirst(ClaimTypes.Name)?.Value
        ?? "—";

    private long? CurrentUserId() =>
        long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
}
