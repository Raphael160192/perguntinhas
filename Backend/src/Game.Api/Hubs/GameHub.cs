using Microsoft.AspNetCore.SignalR;

namespace Game.Api.Hubs;

// Hub de tempo real: cada partida é um grupo identificado pelo gameId.
// Os broadcasts são disparados pelos controllers via IHubContext<GameHub>.
public class GameHub : Hub
{
    public async Task JoinGameGroup(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
    }

    public async Task LeaveGameGroup(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
    }
}
