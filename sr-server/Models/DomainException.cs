namespace SignalRDemo.Server.Models;

public class DomainException : Exception
{
    public IEnumerable<string> ValidationErrors { get; init; }

    public DomainException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
        ValidationErrors = [];
    }

    public DomainException(string message, IEnumerable<string> validationErrors,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ValidationErrors = validationErrors;
    }
}