using Kela.Core.Modules.Auth;
using Microsoft.AspNetCore.SignalR;

namespace Kela.Api.Modules.Feedback.Hubs;

/// <summary>Geri bildirişlər: superadminlər canlı bildiriş alır.</summary>
public sealed class FeedbackHub : Hub
{
    public const string SuperAdminsGroup = "superadmins";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole(AuthRoles.SuperAdmin) == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, SuperAdminsGroup);

        await base.OnConnectedAsync();
    }
}
