namespace SimpleVote.Shared.Models;

public class InvocationResult
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }

    public static InvocationResult Success()
    {
        return new()
        {
            IsSuccess = true,
        };
    }

    public static InvocationResult Failed(string message)
    {
        return new()
        {
            IsSuccess = false,
            Message = message
        };
    }
}