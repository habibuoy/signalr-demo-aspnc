using SignalRDemo.Shared;

namespace SignalRDemo.Server;

public interface IVoteHubClient
{
    Task NotifyVoteCreated(VoteCreatedProperties properties);
    Task NotifyVoteUpdated(VoteUpdatedProperties properties);
}