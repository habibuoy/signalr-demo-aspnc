using System.Diagnostics.CodeAnalysis;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Models;

public class Vote
{
    public required string Id { get; init; }
    public required string Title { get; set; } = string.Empty;
    public List<VoteSubject> Subjects { get; set; } = new();
    public DateTime CreatedTime { get; set; }
    public DateTime? ExpiredTime { get; set; }
    public int? MaximumCount { get; set; }
    public int CurrentTotalCount { get; set; }

    public Dictionary<int, int> SubjectVoteCounts { get; set; } = new();

    public object voteLock = new();

    [SetsRequiredMembers]
    protected Vote()
    {
        Id = Guid.NewGuid().ToString();
    }

    [SetsRequiredMembers]
    protected Vote(string title, string[] subjects)
        : this()
    {
        ArgumentNullException.ThrowIfNull(subjects);
        Title = title;
        for (int i = 0; i < subjects.Length; i++)
        {
            Subjects.Add(new VoteSubject()
            {
                Id = i,
                Name = subjects[i]
            });
        }
    }

    [SetsRequiredMembers]
    public Vote(string title, string[] subjects,
        DateTime? expiredTime = null, int? maximumCount = null)
            : this(title, subjects)
    {
        ExpiredTime = expiredTime;
        MaximumCount = maximumCount;

        CreatedTime = DateTime.UtcNow;
        foreach (var subject in Subjects)
        {
            SubjectVoteCounts.Add(subject.Id, 0);
        }
    }

    public bool IsClosed()
    {
        return ExpiredTime != null
            && DateTime.UtcNow >= ExpiredTime;
    }

    public bool CanVote()
    {
        lock (voteLock)
        {    
            return MaximumCount == null
                || CurrentTotalCount < MaximumCount;
        }
    }

    public void GiveVote(int subjectId)
    {
        lock (voteLock)
        {
            if (SubjectVoteCounts.TryGetValue(subjectId, out int value))
            {
                SubjectVoteCounts[subjectId] = ++value;
                CurrentTotalCount++;
            }
        }
    }
}

public class VoteSubject
{
    public required int Id { get; set; }
    public required string Name { get; set; }
}

public class VoteCount
{
    public required int Id { get; set; }
    public required int Count { get; set; }
}

public static class VoteExtensions
{
    public static VoteDto ToDto(this Vote vote)
    {
        return new VoteDto()
        {
            Id = vote.Id,
            Title = vote.Title,
            Subjects = vote.Subjects,
            VoteCount = vote.SubjectVoteCounts.Select(s => new VoteCount()
            {
                Id = s.Key,
                Count = s.Value
            }),
            CreatedTime = vote.CreatedTime.ToLocalTime(),
            ExpiredTime = vote.ExpiredTime.HasValue ? vote.ExpiredTime.Value.ToLocalTime() : null,
            MaximumCount = vote.MaximumCount,
            CurrentTotalCount = vote.CurrentTotalCount
        };
    }
}