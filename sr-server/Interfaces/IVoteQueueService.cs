using SignalRDemo.Server.Models;
using SignalRDemo.Server.Services;

namespace SignalRDemo.Server.Interfaces;

public interface IVoteQueueService
{
    Task<ServiceResult<VoteQueueInput>> QueueVoteAsync(string subjectId, string? userId);
}