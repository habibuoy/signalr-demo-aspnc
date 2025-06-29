using SignalRDemo.Server.Models;
using static SignalRDemo.Server.Validations.Validators;

namespace SignalRDemo.Server.Validations;

public static class VoteValidators
{
    public static void VoteIdValidator(string voteId, List<string> errors)
    {
        if (voteId == null)
        {
            errors.Add("Vote Id cannot be null");
        }
    }

    public static Result<string, List<string>> ValidateTitle(string title)
    {
        try
        {
            return ValidateModelFieldValue(nameof(title), title, static (title, errors) =>
            {
                if (string.IsNullOrEmpty(title))
                {
                    errors.Add("Title cannot be empty");
                }
                else if (title.Length < Vote.MinimumTitleLength)
                {
                    errors.Add($"Title length cannot be less than {Vote.MinimumTitleLength} characters");
                }
            });
        }
        catch (ModelFieldValidatorException)
        {
            throw;
        }
    }

    public static Result<string[], List<string>> ValidateSubjects(string[] subjects)
    {
        try
        {
            return ValidateModelFieldValue(nameof(subjects), subjects, static (subjects, errors) =>
            {
                if (subjects == null)
                {
                    errors.Add("Subject cannot be null");
                }
                else if (subjects.Length < Vote.MinimumSubjectCount)
                {
                    errors.Add($"Subject count cannot be less than {Vote.MinimumSubjectCount}");
                }
            });
        }
        catch (ModelFieldValidatorException)
        {
            throw;
        }
    }

    public static Result<int?, List<string>> ValidateMaximumCount(int? maximumCount, string[] subjects)
    {
        try
        {
            int subjectCount = subjects == null ? 0 : subjects.Length;

            return ValidateModelFieldValue(nameof(maximumCount), maximumCount,
                subjectCount, static (maximumCount, subjectCount, errors) =>
            {
                if (maximumCount == null) return;

                if (maximumCount < 0)
                {
                    errors.Add("Maximum count cannot be less than zero");
                }

                if (maximumCount > int.MaxValue)
                {
                    errors.Add($"Maximum count cannot be more than {int.MaxValue}");
                }

                if (subjectCount == 0)
                {
                    errors.Add($"Subject count cannot be 0");
                }
                else if (maximumCount < subjectCount)
                {
                    errors.Add($"Maximum count cannot be less than subject count ({subjectCount})");
                }
                else if (maximumCount % subjectCount != 0)
                {
                    errors.Add("Remainder of maximum count divided by subject count cannot be not 0");
                }
            });
        }
        catch (ModelFieldValidatorException)
        {
            throw;
        }
    }

    public static Result<int?, List<string>> ValidateDuration(int? duration)
    {
        try
        {
            return ValidateModelFieldValue(nameof(duration), duration, static (duration, errors) =>
            {
                if (duration == null) return;

                if (duration < 0)
                {
                    errors.Add("Duration cannot be less than zero");
                }

                if (duration > int.MaxValue)
                {
                    errors.Add($"Duration count cannot be more than {int.MaxValue}");
                }
            });
        }
        catch (ModelFieldValidatorException)
        {
            throw;
        }
    }

    public static Result<string, List<string>> ValidateVoteId(string voteId)
    {
        try
        {
            return ValidateModelFieldValue(nameof(voteId), voteId, VoteIdValidator);
        }
        catch (ModelFieldValidatorException)
        {
            throw;
        }
    }
}