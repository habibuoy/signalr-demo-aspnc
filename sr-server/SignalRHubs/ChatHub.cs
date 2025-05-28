using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Shared;

namespace SignalRDemo.Server.SignalRHubs;

public class ChatHub : Hub
{
    public async Task Send(SendMessageProperties properties)
    {
        await Clients.All.SendAsync("ReceiveMessage", properties);
    }
}