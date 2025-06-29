using SimpleVote.Shared;

namespace SimpleVote.Server.Interfaces;

public interface IVoteHubClient
{
    Task ReceiveMessage(SendMessageProperties properties);
    Task NotifyVoteCreated(VoteCreatedProperties properties);
    Task NotifyVoteUpdated(VoteUpdatedProperties properties);
}