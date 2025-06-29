namespace SimpleVote.Server.Endpoints.Responses;

public record RoleResponse(string Id, string Name, string NormalizedName, string? Description,
    DateTime CreatedTime);