namespace SignalRDemo.Server.Models;

public class DomainValidationException : DomainException
{
    public IEnumerable<string> ValidationErrors { get; init; }
    
    public DomainValidationException(string message, IEnumerable<string> validationErrors,
        Exception? innerException = null) : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }
}