using SimpleVote.Server.Validations;
using static SimpleVote.Server.Validations.RoleValidators;

namespace SimpleVote.Server.Endpoints.Requests;

public record CreateRoleRequest(string Name, string? Description) : BaseRequest
{
    public override FieldValidationResult Validate(object? reference = null)
    {
        var validationResult = FieldValidationResult.Create();
        if (ValidateName(Name) is { Succeeded: false } nameValidation)
            validationResult.AddError(nameof(Name), nameValidation.Error);

        return validationResult;
    }
}