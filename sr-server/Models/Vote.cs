using System.Diagnostics.CodeAnalysis;
using SignalRDemo.Server.Utils.Validators;
using static SignalRDemo.Server.Utils.Validators.VoteValidators;

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

    private Vote() { }
    
    [SetsRequiredMembers]
    private Vote(string title, string[] subjects, User? creator,
        int? duration = null, int? maximumCount = null)
    {
        Id = Guid.NewGuid().ToString();

        try
        {
            List<string> validationErrors = new();
            if (ValidateTitle(title) is { Succeeded: false } titleValidation)
                validationErrors.AddRange(titleValidation.Error);

            if (ValidateSubjects(subjects) is { Succeeded: false } subjectsValidation)
                validationErrors.AddRange(subjectsValidation.Error);

            if (ValidateMaximumCount(maximumCount, subjects) is { Succeeded: false } maxCountValidation)
                validationErrors.AddRange(maxCountValidation.Error);

            if (ValidateDuration(duration) is { Succeeded: false } durationValidation)
                validationErrors.AddRange(durationValidation.Error);

            if (validationErrors.Count > 0)
            {
                throw new DomainException($"Validation error while creating {nameof(Vote)} entity. " +
                    "Check out the errors property.", validationErrors);
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            throw new DomainException($"Validator error happened while creating {nameof(Vote)} entity", ex);
        }

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