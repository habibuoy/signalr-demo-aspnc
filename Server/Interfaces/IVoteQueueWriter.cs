using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteQueueWriter
{
    Task WriteAsync(VoteQueueItem vote);
    Task CloseAsync();
}