namespace SignalRDemo.Server.Models;

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message, IEnumerable<string> validationErrors,
        Exception? innerException = null) : base(message, validationErrors, innerException)
    {
        
    }
}