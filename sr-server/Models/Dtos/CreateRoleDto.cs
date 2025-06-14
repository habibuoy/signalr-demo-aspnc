using static SignalRDemo.Server.Utils.Validators.RoleValidators;

namespace SignalRDemo.Server.Models.Dtos;

public class CreateRoleDto : BaseDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }

    public CreateRoleDto(string name, string? description = null)
    {
        Name = name;
        Description = description;
    }

    public override FieldValidationResult Validate(object? reference = null)
    {
        var validationResult = FieldValidationResult.Create();
        if (ValidateName(Name) is { Succeeded: false } nameValidation)
            validationResult.AddError(nameof(Name), nameValidation.Error);

        return validationResult;
    }
}