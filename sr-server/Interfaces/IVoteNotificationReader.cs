using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteNotificationReader
{
    Task<Vote> ReadAsync();
}