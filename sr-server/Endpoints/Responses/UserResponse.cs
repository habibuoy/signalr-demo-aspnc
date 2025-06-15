namespace SignalRDemo.Server.Endpoints.Responses;

public record UserResponse(string Id, string Email, string? FirstName, string? LastName);