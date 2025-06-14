using static SignalRDemo.Server.Utils.Validators.RoleValidators;

namespace SignalRDemo.Server.Models.Dtos;

public class UpdateRoleDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;

    public override FieldValidationResult Validate(object? reference = null)
    {
        var validationResult = FieldValidationResult.Create();
        if (ValidateName(Name) is { Succeeded: false } nameValidation)
            validationResult.AddError(nameof(Name), nameValidation.Error);

        return validationResult;
    }
}