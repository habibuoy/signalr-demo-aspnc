using SimpleVote.Server.Models;

namespace SimpleVote.Server.Interfaces;

public interface IVoteNotificationWriter
{
    Task WriteCreateAsync(Vote vote);
    Task WriteUpdateAsync(Vote vote);
    Task CloseAsync();
}