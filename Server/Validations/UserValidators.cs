using System.Text.RegularExpressions;
using static SimpleVote.Server.Validations.Validators;
using static SimpleVote.Server.Configurations.AppConstants;

namespace SimpleVote.Server.Validations;

public static partial class UserValidator
{
    private const string PasswordOneUppercaseRequirementRegexString = "(?=.*?[A-Z])";
    private const string PasswordOneLowercaseRequirementRegexString = "(?=.*?[a-z])";
    private const string PasswordOneNumberRequirementRegexString = "(?=.*?[0-9])";
    private const string PasswordOneSpecialCharacterRequirementRegexString = "(?=.*?[#?!@$%^&*-])";


    [GeneratedRegex(PasswordOneUppercaseRequirementRegexString)]
    private static partial Regex PasswordOneUppercaseRequirementRegex();

    [GeneratedRegex(PasswordOneLowercaseRequirementRegexString)]
    private static partial Regex PasswordOneLowercaseRequirementRegex();

    [GeneratedRegex(PasswordOneNumberRequirementRegexString)]
    private static partial Regex PasswordOneNumberRequirementRegex();

    [GeneratedRegex(PasswordOneSpecialCharacterRequirementRegexString)]
    private static partial Regex PasswordOneSpecialCharacterRequirementRegex();

    public static Result<string, List<string>> ValidateEmail(string email)
    {
        return ValidateModelFieldValue(nameof(email), email, static (email, errors) =>
        {
            if (email.Length == 0)
            {
                errors.Add("Email cannot be empty");
            }

            if (!email.Contains('@'))
            {
                errors.Add("A valid email contains an @ character");
            }
        });
    }

    public static Result<string, List<string>> ValidatePassword(string password)
    {
        return ValidateModelFieldValue(nameof(password), password, static (password, errors) =>
        {
            if (password.Length == 0)
            {
                errors.Add("Password cannot be empty");
            }

            if (password.Length < UserPasswordMinimumLength)
            {
                errors.Add($"Password should be at least {UserPasswordMinimumLength} characters");
            }

            if (!PasswordOneUppercaseRequirementRegex().IsMatch(password))
            {
                errors.Add("Password should contain at least one uppercase character");
            }

            if (!PasswordOneLowercaseRequirementRegex().IsMatch(password))
            {
                errors.Add("Password should contain at least one lowercase character");
            }

            if (!PasswordOneNumberRequirementRegex().IsMatch(password))
            {
                errors.Add("Password should contain at least one number");
            }

            if (!PasswordOneSpecialCharacterRequirementRegex().IsMatch(password))
            {
                errors.Add("Password should contain at least one special character (#?!@$%^&*-)");
            }
        });
    }

    public static Result<string, List<string>> ValidatePasswordAgainstEmail(string password, string email)
    {
        return ValidateModelFieldValue(nameof(password), password, email, static (password, email, errors) =>
        {
            if (email == password)
            {
                errors.Add("Password should not be the same as email");
            };
        });
    }
}