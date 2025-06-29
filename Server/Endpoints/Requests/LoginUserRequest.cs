namespace SimpleVote.Server.Endpoints.Requests;

public record LoginUserRequest(string Email, string Password);