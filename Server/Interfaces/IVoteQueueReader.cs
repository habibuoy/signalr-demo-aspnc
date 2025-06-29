using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteQueueReader
{
    Task<VoteQueueItem> ReadAsync();
}