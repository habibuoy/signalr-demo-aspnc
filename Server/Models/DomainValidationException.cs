using SimpleVote.Server.Utils.Extensions;

namespace SimpleVote.Server.Models;

public class DomainValidationException : DomainException
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ValidationErrors { get; init; }

    public DomainValidationException(string message,
        IReadOnlyDictionary<string, IReadOnlyList<string>> validationErrors,
        Exception? innerException = null) : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }

    public DomainValidationException(string message,
        Dictionary<string, List<string>> validationErrors,
        Exception? innerException = null) : base(message, innerException)
    {
        ValidationErrors = validationErrors.ToReadOnly();
    }
}