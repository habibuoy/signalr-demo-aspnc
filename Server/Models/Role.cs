using SimpleVote.Server.Validations;
using static SimpleVote.Server.Validations.RoleValidators;

namespace SimpleVote.Server.Models;

public class Role
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedTime { get; set; }

    private Role() { }

    private Role(string name)
    {
        try
        {
            var validationErrors = new Dictionary<string, List<string>>();
            if (ValidateName(name) is { Succeeded: false } nameValidation)
                validationErrors.Add(nameof(name), nameValidation.Error);

            if (validationErrors.Count > 0)
            {
                throw new DomainValidationException($"Validation error while creating {nameof(Role)} entity. " +
                    "Check out the errors property.",
                    validationErrors);
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            throw new DomainException($"Validator error happened while creating {nameof(Role)} entity", ex);
        }

        Id = Guid.CreateVersion7().ToString();
        Name = name.ToLower();
        NormalizedName = Name.ToUpper();
        CreatedTime = DateTime.UtcNow;
    }

    public static Role Create(string name, string? description = null)
    {
        return new Role(name)
        {
            Description = description ?? string.Empty
        };
    }
}