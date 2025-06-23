using SignalRDemo.Server.Models;
using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Services;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteService
{
    Task<Vote?> GetVoteByIdAsync(string id);
    Task<IEnumerable<Vote>> GetVotesAsync(int? count = 10,
        string? sortBy = null,
        string? sortOrder = null,
        string? search = null);
    Task<IEnumerable<VoteSubjectInput>> GetVoteInputsByUserIdAsync(string userId);
    Task<IEnumerable<VoteSubjectInput>?> GetVoteInputsByVoteIdAsync(string voteId);
    Task<ServiceResult<Vote>> CreateVoteAsync(Vote vote);
    /// <summary>
    /// Update the vote asynchronously
    /// </summary>
    /// <returns>A boolean to indicate whether the process is successful or not</returns>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    Task<ServiceResult<Vote>> UpdateVoteAsync(string voteId, Vote vote);
    /// <summary>
    /// Give vote asynchronously
    /// </summary>
    /// <returns>A boolean to indicate whether the process is successful or not</returns>
    /// <exception cref="DbUpdateConcurrencyException"></exception>
    Task<ServiceResult<VoteSubjectInput>> GiveVoteAsync(string subjectId, string? userId);
    /// <summary>
    /// Delete vote asynchronously
    /// </summary>
    /// <param name="vote">Vote object</param>
    /// <returns>A boolean to indicate whether the process is successful or not</returns>
    /// <exception cref="ArgumentNullException"></exception>
    Task<ServiceResult<Vote>> DeleteVoteAsync(Vote vote);
}