using static SignalRDemo.Server.Utils.Validators.Validators;
using static SignalRDemo.Server.Configurations.AppConstants;

namespace SignalRDemo.Server.Utils.Validators;

public static class RoleValidators
{
    public static Result<string, List<string>> ValidateName(string name)
    {
        return ValidateModelFieldValue(nameof(name), name, static (name, errors) =>
        {
            if (name.Length == 0)
            {
                errors.Add("Role name cannot be empty");
            }

            if (name.Length < RoleNameMinimumLength)
            {
                errors.Add($"Role name should be at least {RoleNameMinimumLength} characters");
            }
        });
    }
}