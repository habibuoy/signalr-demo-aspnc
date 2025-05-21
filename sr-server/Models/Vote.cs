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
    public int CurrentTotalCount { get; set; }

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
                VoteCount = new VoteCount()
                {
                    Id = i,
                    SubjectId = i,
                    Count = 0
                }
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
            return MaximumCount == null
                || CurrentTotalCount < MaximumCount;
        }
    }

    public void GiveVote(int subjectId)
    {
        lock (voteLock)
        {
            if (Subjects.Find(v => v.Id == subjectId) is VoteSubject voteSubject)
            {
                voteSubject.VoteCount.Count++;
                CurrentTotalCount++;
                voteSubject.VoteCount.Version = Guid.NewGuid();
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
            int count = subject.VoteCount.Count;

            subjects.Add(new()
            {
                Id = subject.Id,
                Name = subject.Name,
                VoteCount = new()
                {
                    Count = count
                }
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
            TotalCount = vote.CurrentTotalCount
        };
    }
}