using System.Diagnostics.CodeAnalysis;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Shared;

namespace SignalRDemo.Server.Models;

public class Vote
{
    public required string Id { get; init; }
    public required string Title { get; set; } = string.Empty;
    public List<VoteSubject> Subjects { get; set; } = new();
    public DateTime CreatedTime { get; set; }
    public DateTime? ExpiredTime { get; set; }
    public int? MaximumCount { get; set; }

    private object voteLock = new();

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
            var subject = subjects[i];

            Subjects.Add(new VoteSubject()
            {
                Id = i,
                Name = subject,
                VoteId = Id,
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
            var currentTotal = Subjects.Aggregate(0, (acc, s) =>
            {
                acc += s.Voters.Count;
                return acc;
            });
            return MaximumCount == null
                || currentTotal < MaximumCount;
        }
    }

    public void GiveVote(int subjectId, string? userId)
    {
        lock (voteLock)
        {
            if (Subjects.Find(v => v.Id == subjectId) is VoteSubject voteSubject)
            {
                var voteInput = new VoteSubjectInput()
                {
                    VoterId = userId,
                    InputTime = DateTime.Now
                };

                voteSubject.Voters.Add(voteInput);
            }
        }
    }
}

public static class VoteExtensions
{
    public static VoteDto ToDto(this Vote vote)
    {
        var subjects = new List<VoteSubjectDto>();
        int totalCount = 0;

        foreach (var subject in vote.Subjects)
        {
            int count = subject.Voters.Count;

            subjects.Add(new()
            {
                Id = subject.Id,
                Name = subject.Name,
                CurrentCount = count
            });

            totalCount += count;
        }
        
        return new VoteDto()
        {
            Id = vote.Id,
            Title = vote.Title,
            Subjects = subjects,
            CreatedTime = vote.CreatedTime.ToLocalTime(),
            ExpiredTime = vote.ExpiredTime.HasValue ? vote.ExpiredTime.Value.ToLocalTime() : null,
            MaximumCount = vote.MaximumCount,
            CurrentTotalCount = totalCount
        };
    }

    public static VoteCreatedProperties ToVoteCreatedProperties(this Vote vote)
    {
        return new VoteCreatedProperties()
        {
            Id = vote.Id,
            Title = vote.Title,
            SubjectCount = vote.Subjects.Count
        };
    }

    public static VoteUpdatedProperties ToVoteUpdatedProperties(this Vote vote)
    {
        return new VoteUpdatedProperties()
        {
            Id = vote.Id,
            Title = vote.Title,
            TotalCount = vote.Subjects.Aggregate(0, (acc, c) =>
            {
                acc += c.Voters.Count;
                return acc;
            })
        };
    }
}