using SimpleVote.Server.Models;
using SimpleVote.Server.Services;

namespace SimpleVote.Server.Interfaces;

public interface IVoteQueueService
{
    Task<ServiceResult<VoteQueueInput>> QueueVoteAsync(string subjectId, string? userId);
}