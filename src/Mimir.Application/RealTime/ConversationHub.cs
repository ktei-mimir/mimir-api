using Microsoft.AspNetCore.SignalR;

namespace Mimir.Application.RealTime;

public class ConversationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        if (!user.Identity.IsAuthenticated)
        {
            throw new HubException("User is not authenticated");
        }
        await base.OnConnectedAsync();
    }
}