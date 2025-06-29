using SimpleVote.Server.Services.Errors;

namespace SimpleVote.Server.Utils.Extensions;

public static class ServiceResultExtensions
{
    public static bool TryToConvertToValidationError(this ServiceError error,
        out ServiceValidationError validationError)
    {
        if (error is not ServiceValidationError valError)
        {
            validationError = null!;
            return false;
        }

        validationError = valError;
        return true;
    }
}