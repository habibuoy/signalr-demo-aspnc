using System.Diagnostics.CodeAnalysis;
using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Validations.VoteValidators;

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
            var validationErrors = new Dictionary<string, List<string>>();
            if (ValidateTitle(title) is { Succeeded: false } titleValidation)
                validationErrors.Add(nameof(title), titleValidation.Error);

            if (ValidateSubjects(subjects) is { Succeeded: false } subjectsValidation)
                validationErrors.Add(nameof(subjects), subjectsValidation.Error);

            if (ValidateMaximumCount(maximumCount, subjects) is { Succeeded: false } maxCountValidation)
                validationErrors.Add(nameof(maximumCount), maxCountValidation.Error);

            if (ValidateDuration(duration) is { Succeeded: false } durationValidation)
                validationErrors.Add(nameof(duration), durationValidation.Error);

            if (validationErrors.Count > 0)
            {
                throw new DomainValidationException($"Validation error while creating {nameof(Vote)} entity. " +
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

    [SetsRequiredMembers]
    private Vote(string title, VoteSubject[] subjects, User? creator,
        int? duration = null, int? maximumCount = null) 
        : this(title, subjects.Select(s => s.Name).ToArray(), creator, duration, maximumCount) { }

    public void UpdateFrom(Vote vote)
    {
        DateTime? updateExpiredDt = null;
        try
        {
            var validationErrors = new Dictionary<string, List<string>>();

            if (ValidateTitle(vote.Title) is { Succeeded: false } titleValidation)
                validationErrors.Add(nameof(vote.Title), titleValidation.Error);

            string[] subjects = vote.Subjects == null ? null! : [.. vote.Subjects.Select(s => s.Name)];
            if (ValidateSubjects(subjects) is { Succeeded: false } subjectsValidation)
                validationErrors.Add(nameof(vote.Subjects), subjectsValidation.Error);

            var maxCount = vote.MaximumCount;
            if (maxCount != null)
            {
                var cCount = CurrentCount;
                if (maxCount.Value < cCount)
                {
                    if (!validationErrors.TryGetValue(nameof(vote.MaximumCount), out var maxCountError))
                    {
                        maxCountError = new();
                        validationErrors.Add(nameof(vote.MaximumCount), maxCountError);
                    }
                    maxCountError.Add($"Maximum count ({maxCount}) cannot be less than current count ({cCount})");
                }

                if (ValidateMaximumCount(maxCount, subjects) is { Succeeded: false } maxCountValidation)
                {
                    if (validationErrors.TryGetValue(nameof(vote.MaximumCount), out var maxCountError))
                    {
                        maxCountError.AddRange(maxCountValidation.Error);
                    }
                    else
                    {
                        validationErrors.Add(nameof(vote.MaximumCount), maxCountValidation.Error);
                    }
                }
            }

            if (vote.ExpiredTime != null)
            {
                var duration = vote.ExpiredTime.Value - vote.CreatedTime;
                if (ValidateDuration(duration.Seconds) is { Succeeded: false } durationValidation)
                    validationErrors.Add(nameof(duration), durationValidation.Error);
                var dtNow = DateTime.UtcNow;
                updateExpiredDt = CreatedTime.Add(duration);
                if (updateExpiredDt < dtNow)
                {
                    if (!validationErrors.TryGetValue(nameof(duration), out var durationError))
                    {
                        durationError = new();
                        validationErrors.Add(nameof(vote.MaximumCount), durationError);
                    }

                    durationError.Add($"Duration should not make " +
                        $"the expired time earlier than now {dtNow}. With current duration ({duration} seconds), " +
                        $"expired time would be on {updateExpiredDt}");
                }
            }

            if (validationErrors.Count > 0)
            {
                throw new DomainValidationException($"Validation error while updating {nameof(Vote)} entity. " +
                    "Check out the errors property.", validationErrors);
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            throw new DomainException($"Validator error happened while updating {nameof(Vote)} entity", ex);
        }

        Title = vote.Title;
        Subjects.ForEach(sub =>
        {
            var matching = vote.Subjects!.Find(s => s.Id == sub.Id);
            if (matching != null)
            {
                sub.Name = matching.Name;
            }
        });
        MaximumCount = vote.MaximumCount;
        ExpiredTime = updateExpiredDt;
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

                var voteInput = VoteSubjectInput.Create(subjectId, userId);
                voteSubject.Voters.Add(voteInput);
            }
        }
    }

    public static Vote Create(string title, string[] subjects, User? creator, int? duration, int? maximumCount)
        => new(title, subjects, creator, duration, maximumCount);

    public static Vote Create(string title, VoteSubject[] subjects, User? creator, int? duration, int? maximumCount)
    {
        var vote = new Vote(title, subjects, creator, duration, maximumCount);
        vote.Subjects = [.. subjects];
        return vote;
    }
}