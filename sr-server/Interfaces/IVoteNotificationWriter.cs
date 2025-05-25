using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteNotificationWriter
{
    Task WriteUpdateAsync(Vote vote);
    Task CloseAsync();
}