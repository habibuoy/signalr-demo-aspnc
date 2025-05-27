using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Shared;
using SignalRDemo.Shared.Models;

namespace SignalRDemo.Server;

public class VoteHub : Hub<IVoteHubClient>
{
    private const string VoteGroupNamePrefix = "vote-";

    private readonly ILogger<VoteHub> logger;

    public VoteHub(ILogger<VoteHub> logger)
    {
        this.logger = logger;
    }

    [Authorize]
    public async Task Send(SendMessageProperties properties)
    {
        await Clients.All.ReceiveMessage(properties);
    }

    [Authorize]
    public async Task<InvocationResult> SubscribeVote(string voteId,
        [FromServices] IVoteService voteService)
    {
        if (Context.User == null)
        {
            return InvocationResult.Failed("Not authorized");
        }

        var userId = Context.UserIdentifier;
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name);
        logger.LogInformation("User {user} is trying to subscribe to vote {voteId}",
            userName, voteId);

        try
        {
            var vote = await voteService.GetVoteByIdAsync(voteId);

            if (vote == null)
            {
                logger.LogInformation("Vote {voteId} does not exist",
                    voteId);
                return InvocationResult.Failed($"Vote {voteId} does not exist");
            }

            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var connectionReader = httpContext.RequestServices.GetRequiredService<IHubConnectionReader>();
                var allConnectedIds = await connectionReader.GetConnectionIdsAsync(userId!);
                var groupName = GetVoteGroupName(voteId);
                foreach (var connId in allConnectedIds)
                {
                    await Groups.AddToGroupAsync(connId, groupName);
                }

                await Clients.Users(userId!).ReceiveMessage(
                    SendMessageProperties.ServerNotification($"You subscribed to vote {voteId}"));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error happened while user {user} subcribing vote {voteId}",
                userId, voteId);
        }

        return InvocationResult.Success();
    }

    [Authorize]
    public async Task<InvocationResult> UnsubscribeVote(string voteId, 
        [FromServices] IVoteService voteService)
    {
        if (Context.User == null)
        {
            return InvocationResult.Failed("Not authorized");
        }
        
        var userId = Context.UserIdentifier;
        var userName = Context.User?.FindFirstValue(ClaimTypes.Name);
        logger.LogInformation("User {user} is unsubscribing to vote {voteId}",
            userName, voteId);

        try
        {
            var vote = await voteService.GetVoteByIdAsync(voteId);

            if (vote == null)
            {
                logger.LogInformation("Vote {voteId} does not exist",
                    voteId);
                return InvocationResult.Failed($"Vote {voteId} does not exist");
            }

            var httpContext = Context.GetHttpContext();
            if (httpContext != null)
            {
                var connectionReader = httpContext.RequestServices.GetRequiredService<IHubConnectionReader>();
                var allConnectedIds = await connectionReader.GetConnectionIdsAsync(userId!);
                var groupName = GetVoteGroupName(voteId);
                foreach (var connId in allConnectedIds)
                {
                    await Groups.RemoveFromGroupAsync(connId, groupName);
                }

                await Clients.Users(userId!).ReceiveMessage(
                    SendMessageProperties.ServerNotification($"You unsubscribed to vote {voteId}"));
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetVoteGroupName(voteId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error happened while user {user} unsubcribing vote {voteId}",
                userId, voteId);
        }

        return InvocationResult.Success();
    }

    public static string GetVoteGroupName(string voteId)
    {
        return $"{VoteGroupNamePrefix}{voteId}";
    }

    public override Task OnConnectedAsync()
    {
        if (Context.GetHttpContext() is HttpContext httpContext)
        {
            var connectionId = Context.ConnectionId;

            var connectionManager = httpContext.RequestServices.GetRequiredService<IHubConnectionManager>();
            connectionManager.AddConnectionIdAsync(Context.UserIdentifier!, connectionId);
        }
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.GetHttpContext() is HttpContext httpContext)
        {
            var connectionId = Context.ConnectionId;

            var connectionManager = httpContext.RequestServices.GetRequiredService<IHubConnectionManager>();
            connectionManager.RemoveConnectionIdAsync(Context.UserIdentifier!, connectionId);
        }
        return base.OnDisconnectedAsync(exception);
    }
}