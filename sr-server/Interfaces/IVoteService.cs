using SignalRDemo.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteService
{
    Task<Vote?> GetVoteByIdAsync(string id);
    Task<IEnumerable<Vote>> GetVotesAsync(int? count = 10,
        string? sortBy = null,
        string? sortOrder = null,
        Func<Vote, bool>? predicate = null);
    Task<IEnumerable<VoteSubjectInput>> GetVoteInputsByUserIdAsync(string userId);
    Task<IEnumerable<VoteSubjectInput>?> GetVoteInputsByVoteIdAsync(string voteId);
    Task<bool> AddVoteAsync(Vote vote);
    Task<bool> RemoveVoteAsync(string id);
    /// <summary>
    /// Update the vote asynchronously
    /// </summary>
    /// <returns>A boolean to indicate whether the process is successful or not</returns>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    Task<bool> UpdateVoteAsync(string voteId, Vote vote);
    /// <summary>
    /// Give vote asynchronously
    /// </summary>
    /// <returns>A boolean to indicate whether the process is successful or not</returns>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    Task<bool> GiveVoteAsync(string subjectId, string? userId);
    /// <summary>
    /// Delete vote asynchronously
    /// </summary>
    /// <param name="vote">Vote object</param>
    /// <returns>A boolean to indicate whether the process is successful or not</returns>
    /// <exception cref="ArgumentNullException"></exception>
    Task<bool> DeleteVoteAsync(Vote vote);
}