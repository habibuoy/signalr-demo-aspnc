using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteNotificationReader
{
    Task<Vote> ReadCreatedNotificationAsync();
    Task<Vote> ReadUpdatedNotificationAsync();
}