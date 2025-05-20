using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteService
{
    Task<Vote?> GetVoteByIdAsync(string id);
    Task<bool> AddVoteAsync(Vote vote);
    Task<bool> RemoveVoteAsync(string id);
    Task<bool> UpdateVoteAsync(string voteId, Vote vote);
}