namespace SignalRDemo.Server.Models;

public class DomainValidationException : DomainException
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ValidationErrors { get; init; }
    
    public DomainValidationException(string message,
        IReadOnlyDictionary<string, IReadOnlyList<string>> validationErrors,
        Exception? innerException = null) : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }
}