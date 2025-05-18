using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Shared;

namespace SignalRDemo.Server;

public class ChatHub : Hub
{
    public async Task Send(SendMessageProperties properties)
    {
        await Clients.All.SendAsync("ReceiveMessage", properties);
    }
}