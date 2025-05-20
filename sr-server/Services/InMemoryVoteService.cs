using System.Collections.Concurrent;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Services;

public class InMemoryVoteService : IVoteService
{
    private static readonly ConcurrentDictionary<string, Vote> votes = new();
    private readonly ILogger<InMemoryVoteService> logger;

    public InMemoryVoteService(ILogger<InMemoryVoteService> logger)
    {
        this.logger = logger;
    }

    public Task<bool> AddVoteAsync(Vote vote)
    {
        var result = votes.TryAdd(vote.Id, vote);
        return Task.FromResult(result);
    }

    public Task<Vote?> GetVoteByIdAsync(string id)
    {
        if (!votes.TryGetValue(id, out var vote))
        {
            return Task.FromResult<Vote?>(null);
        }

        return Task.FromResult<Vote?>(vote);
    }

    public Task<bool> UpdateVoteAsync(string voteId, Vote vote)
    {
        if (!votes.TryGetValue(voteId, out var existing))
        {
            return Task.FromResult(false);
        }

        existing.Subjects = vote.Subjects;
        existing.Title = vote.Title;
        existing.SubjectVoteCounts = vote.SubjectVoteCounts;
        return Task.FromResult(true);
    }

    public Task<bool> RemoveVoteAsync(string id)
    {
        if (!votes.TryRemove(id, out var vote))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}