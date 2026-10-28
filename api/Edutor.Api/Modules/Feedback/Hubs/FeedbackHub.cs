using Edutor.Core.Modules.Auth;
using Microsoft.AspNetCore.SignalR;

namespace Edutor.Api.Modules.Feedback.Hubs;

/// <summary>Geri bildiriş söhbətləri üçün canlı kanal.</summary>
public sealed class FeedbackHub : Hub
{
    public const string SuperAdminsGroup = "superadmins";

    public static string UserGroup(long userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var id = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(id, out var userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

        if (Context.User?.IsInRole(AuthRoles.SuperAdmin) == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, SuperAdminsGroup);

        await base.OnConnectedAsync();
    }
}
