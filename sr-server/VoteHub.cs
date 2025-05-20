using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server.Interfaces;

namespace SignalRDemo.Server;

public class VoteHub : Hub<IVoteHubClient>
{
    private readonly ILogger<VoteHub> logger;

    public VoteHub(ILogger<VoteHub> logger)
    {
        this.logger = logger;
    }

    public async Task<bool> SubscribeVote(string user, string voteId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"vote-{voteId}");
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> UnsubscribeVote(string user, string voteId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"vote-{voteId}");
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    // public override Task OnConnectedAsync()
    // {
    //     if (Context.GetHttpContext() is HttpContext httpContext)
    //     {
    //         var connectionId = Context.ConnectionId;
    //         logger.LogInformation("Connection {conId} has http context", connectionId);
    //         if (httpContext.Request.Query["autoSubs"] is StringValues autoSub
    //             && autoSub != StringValues.Empty)
    //         {
    //             logger.LogInformation("Connection {conId} has auto sub query", connectionId);
    //             Groups.AddToGroupAsync(connectionId, "AutoSubscribers");
    //         }
    //     }
    //     return base.OnConnectedAsync();
    // }
}