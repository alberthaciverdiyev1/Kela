using System.Security.Claims;
using Kela.Api.Common.Controllers;
using Kela.Api.Common.Responses;
using Kela.Api.Modules.Feedback.Hubs;
using Kela.Core.Modules.Feedback.Abstractions;
using Kela.Core.Modules.Feedback.Requests;
using Kela.Core.Modules.Feedback.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Kela.Api.Modules.Feedback.Controllers;

[Route("api/[controller]")]
public sealed class FeedbackController(IFeedbackService service, IHubContext<FeedbackHub> hub) : ApiControllerBase
{
    [HttpPost]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<FeedbackResponse>>> Create(
        [FromForm] CreateFeedbackRequest request, CancellationToken ct)
    {
        // Göndərən istifadəçi token-dən götürülür — müştəri öz adını yaza bilməz.
        request.UserId = long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
        request.UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "—";
        request.UserEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        request.Role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        var feedback = await service.CreateAsync(request, ct);

        // Admin paneldə inbox canlı yenilənsin.
        await hub.Clients.Group(FeedbackHub.SuperAdminsGroup).SendAsync("feedback:new", feedback, ct);

        return Success(feedback);
    }
}
