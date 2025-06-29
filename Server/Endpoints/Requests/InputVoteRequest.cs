using SimpleVote.Server.Validations;
using static SimpleVote.Server.Validations.VoteValidators;

namespace SimpleVote.Server.Endpoints.Requests;

public record InputVoteRequest(string VoteId, int SubjectId) : BaseRequest
{
    public override FieldValidationResult Validate(object? reference = null)
    {
        var result = FieldValidationResult.Create();
        if (ValidateVoteId(VoteId) is { Succeeded: false } voteIdValidation)
            result.AddError(nameof(VoteId), voteIdValidation.Error);

        return result;
    }
}