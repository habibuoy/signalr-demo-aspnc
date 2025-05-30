using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteNotificationWriter
{
    Task WriteCreateAsync(Vote vote);
    Task WriteUpdateAsync(Vote vote);
    Task CloseAsync();
}