using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Validations.VoteValidators;

namespace SignalRDemo.Server.Endpoints.Requests;

public record UpdateVoteRequest(string Title, UpdateVoteRequest.VoteSubject[] Subjects,
    int? MaximumCount, int? Duration) : BaseRequest
{
    public override FieldValidationResult Validate(object? reference = null)
    {
        var result = FieldValidationResult.Create();

        try
        {
            if (ValidateTitle(Title) is { Succeeded: false } titleValidation)
                result.AddError(nameof(Title), titleValidation.Error);

            var subjects = Subjects.Select(s => s.Name).ToArray();

            if (ValidateSubjects(subjects) is { Succeeded: false } subjectsValidation)
                result.AddError(nameof(Subjects), subjectsValidation.Error);
            if (ValidateMaximumCount(MaximumCount, subjects) is { Succeeded: false } maxCountValidation)
                result.AddError(nameof(MaximumCount), maxCountValidation.Error);
            if (ValidateDuration(Duration) is { Succeeded: false } durationValidation)
                result.AddError(nameof(Duration), durationValidation.Error);
        }
        catch (ValidatorException)
        {
            throw;
        }

        return result;
    }
    
    public record VoteSubject(int Id, string Name);
}