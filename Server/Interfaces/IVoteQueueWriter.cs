using SimpleVote.Server.Models;

namespace SimpleVote.Server.Interfaces;

public interface IVoteQueueWriter
{
    Task WriteAsync(VoteQueueItem vote);
    Task CloseAsync();
}