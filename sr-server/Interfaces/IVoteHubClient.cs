using SignalRDemo.Shared;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteHubClient
{
    Task NotifyVoteCreated(VoteCreatedProperties properties);
    Task NotifyVoteUpdated(VoteUpdatedProperties properties);
}