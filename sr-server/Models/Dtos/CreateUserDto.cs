using static SignalRDemo.Server.Utils.Validators.UserValidator;

namespace SignalRDemo.Server.Models.Dtos;

public class CreateUserDto : BaseDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public override FieldValidationResult Validate(object? reference = null)
    {
        var validationResult = FieldValidationResult.Create();

        var emailValidation = ValidateEmail(Email);
        if (!emailValidation.Succeeded)
            validationResult.AddError(nameof(Email), emailValidation.Error);

        var passwordValidation = ValidatePassword(Password);
        if (!passwordValidation.Succeeded)
            validationResult.AddError(nameof(Password), passwordValidation.Error);

        if (emailValidation.Succeeded && passwordValidation.Succeeded)
        {
            if (ValidatePasswordAgainstEmail(Password, Email) is { Succeeded: false, Error: var errors })
            {
                validationResult.AddError(nameof(Password), errors);
            }
        }

        return validationResult;
    }
}