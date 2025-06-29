namespace SimpleVote.Server.Models;

public class DomainException : Exception
{
    public DomainException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}