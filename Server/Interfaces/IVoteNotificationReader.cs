using SimpleVote.Server.Models;

namespace SimpleVote.Server.Interfaces;

public interface IVoteNotificationReader
{
    Task<Vote> ReadCreatedNotificationAsync();
    Task<Vote> ReadUpdatedNotificationAsync();
}