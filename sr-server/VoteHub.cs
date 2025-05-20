using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server.Interfaces;

namespace SignalRDemo.Server;

public class VoteHub : Hub<IVoteHubClient>
{
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
}