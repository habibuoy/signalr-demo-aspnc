using SignalRDemo.Server.Utils;
using System.Diagnostics.CodeAnalysis;
using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Validations.UserValidator;

namespace SignalRDemo.Server.Models;

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedTime { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    private User() { }

    [SetsRequiredMembers]
    private User(string email, string password, string? firstName, string? lastName)
    {
        try
        {
            Dictionary<string, List<string>> validationErrors = new();
            var emailValidation = ValidateEmail(email);
            if (!emailValidation.Succeeded)
                validationErrors.Add(nameof(email), emailValidation.Error);

            var passwordValidation = ValidatePassword(password);
            if (!passwordValidation.Succeeded)
                validationErrors.Add(nameof(password), passwordValidation.Error);

            if (emailValidation.Succeeded && passwordValidation.Succeeded)
            {
                if (ValidatePasswordAgainstEmail(password, email) is { Succeeded: false, Error: var errors })
                {
                    if (!validationErrors.TryGetValue(nameof(password), out var passwordError))
                    {
                        passwordError = new();
                        validationErrors.Add(nameof(password), passwordError);
                    }
                    
                    passwordError.AddRange(errors);
                }
            }

            if (validationErrors.Count > 0)
            {
                throw new DomainValidationException($"Validation error while creating {nameof(User)} entity. " +
                    "Check out the errors property.",
                    (IReadOnlyDictionary<string, IReadOnlyList<string>>)validationErrors);
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            throw new DomainException($"Validator error happened while creating {nameof(User)} entity", ex);
        }

        Id = Guid.CreateVersion7().ToString();

        Email = email;
        PasswordHash = PasswordHasher.Hash(password);
        FirstName = firstName;
        LastName = lastName;
        CreatedTime = DateTime.UtcNow;
    }

    public static User Create(string email, string password, string? firstName, string? lastName)
        => new(email, password, firstName, lastName);
}