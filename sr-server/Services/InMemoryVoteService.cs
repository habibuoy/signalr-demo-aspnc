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

    public Task<IEnumerable<VoteSubjectInput>> GetVoteInputsByUserIdAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var voteInputs = votes.Values
            .SelectMany(v => v.Subjects)
            .SelectMany(s => s.Voters)
            .Where(si => si.VoterId == userId);

        return Task.FromResult<IEnumerable<VoteSubjectInput>>(voteInputs.ToList());
    }

    public Task<IEnumerable<VoteSubjectInput>?> GetVoteInputsByVoteIdAsync(string voteId)
    {
        ArgumentException.ThrowIfNullOrEmpty(voteId);

        var vote = votes.Values.FirstOrDefault(v => v.Id == voteId);
        if (vote == null)
        {
            return Task.FromResult<IEnumerable<VoteSubjectInput>?>(null);
        }

        var inputs = vote.Subjects.Aggregate(new List<VoteSubjectInput>(), (acc, e) =>
        {
            acc.AddRange(e.Voters);
            return acc;
        });

        return Task.FromResult<IEnumerable<VoteSubjectInput>?>(inputs);
    }

    public Task<IEnumerable<Vote>> GetVotesAsync(int? count = 10,
        string? sortBy = null,
        string? sortOrder = null,
        Func<Vote, bool>? predicate = null)
    {
        if (predicate != null)
        {
            return Task.FromResult(votes.Values.Where(predicate));
        }

        return Task.FromResult<IEnumerable<Vote>>(votes.Values);
    }

    public Task<bool> UpdateVoteAsync(string voteId, Vote vote)
    {
        if (!votes.TryGetValue(voteId, out var existing))
        {
            return Task.FromResult(false);
        }

        existing.Subjects = vote.Subjects;
        existing.Title = vote.Title;
        existing.Subjects = vote.Subjects;
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

    public Task<bool> GiveVoteAsync(string subjectId, string? userId)
    {
        int sid = int.Parse(subjectId);
        var vote = votes.Values.FirstOrDefault(v => v.Subjects.Any(s => s.Id == sid));
        if (vote == null)
        {
            return Task.FromResult(false);
        }

        vote.GiveVote(sid, userId);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteVoteAsync(Vote vote)
    {
        ArgumentNullException.ThrowIfNull(vote);

        return Task.FromResult(votes.TryRemove(vote.Id, out _));
    }
}