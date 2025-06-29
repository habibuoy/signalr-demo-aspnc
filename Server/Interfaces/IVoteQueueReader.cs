using SimpleVote.Server.Models;

namespace SimpleVote.Server.Interfaces;

public interface IVoteQueueReader
{
    Task<VoteQueueItem> ReadAsync();
}