using SignalRDemo.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteService
{
    Task<Vote?> GetVoteByIdAsync(string id);
    Task<bool> AddVoteAsync(Vote vote);
    Task<bool> RemoveVoteAsync(string id);
    /// <summary>
    /// Update the vote asynchronously
    /// </summary>
    /// <returns>A boolean to indicate whether the process is success or not</returns>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    Task<bool> UpdateVoteAsync(string voteId, Vote vote);
}