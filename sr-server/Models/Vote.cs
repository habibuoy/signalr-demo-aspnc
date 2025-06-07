using System.Diagnostics.CodeAnalysis;

namespace SignalRDemo.Server.Models;

public class Vote
{
    public const int MinimumTitleLength = 3;
    public const int MinimumSubjectCount = 2;
    
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
    public int CurrentCount => Subjects.Aggregate(0, (acc, n) =>
    {
        acc += n.Voters.Count;
        return acc;
    });


    [SetsRequiredMembers]
    protected Vote()
    {
        Id = Guid.NewGuid().ToString();
    }

    [SetsRequiredMembers]
    private Vote(string title, string[] subjects)
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
    private Vote(string title, string[] subjects, User? creator,
        int? duration = null, int? maximumCount = null)
            : this(title, subjects)
    {
        MaximumCount = maximumCount;
        CreatedTime = DateTime.UtcNow;

        ExpiredTime = duration != null && duration > 0 ? CreatedTime.AddSeconds(duration.Value) : null;
        CreatorId = creator?.Id;
        User = creator;
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

    public static Vote Create(string title, string[] subjects, User? creator, int? duration, int? maximumCount) =>
        new(title, subjects, creator, duration, maximumCount);
}