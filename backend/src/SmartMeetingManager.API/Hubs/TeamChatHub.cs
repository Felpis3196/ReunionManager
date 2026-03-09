using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace SmartMeetingManager.API.Hubs;

[Authorize]
public class TeamChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var organizationId = Context.User?.FindFirst("organizationId")?.Value;
        if (!string.IsNullOrEmpty(organizationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "org_" + organizationId);
        }
        await base.OnConnectedAsync();
    }
}
