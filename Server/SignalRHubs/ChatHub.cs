using Microsoft.AspNetCore.SignalR;
using SimpleVote.Shared;

namespace SimpleVote.Server.SignalRHubs;

public class ChatHub : Hub
{
    public async Task Send(SendMessageProperties properties)
    {
        await Clients.All.SendAsync("ReceiveMessage", properties);
    }
}