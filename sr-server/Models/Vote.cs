using System.Diagnostics.CodeAnalysis;

namespace SignalRDemo.Server.Models;

public class Vote
{
    public required string Id { get; init; }
    public required string Title { get; set; } = string.Empty;
    public List<VoteSubject> Subjects { get; set; } = new();
    public DateTime CreatedTime { get; set; }
    public DateTime? ExpiredTime { get; set; }
    public int? MaximumCount { get; set; }
    public string? CreatorId { get; set; }

    // navigational
    public User? User { get; set; }

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
                    InputTime = DateTime.UtcNow
                };

                voteSubject.Voters.Add(voteInput);
            }
        }
    }
}