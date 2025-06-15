using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Validations.RoleValidators;

namespace SignalRDemo.Server.Endpoints.Requests;

public record UpdateRoleRequest(string Name, string Description) : BaseRequest
{
    public override FieldValidationResult Validate(object? reference = null)
    {
        var validationResult = FieldValidationResult.Create();
        if (ValidateName(Name) is { Succeeded: false } nameValidation)
            validationResult.AddError(nameof(Name), nameValidation.Error);

        return validationResult;
    }
}