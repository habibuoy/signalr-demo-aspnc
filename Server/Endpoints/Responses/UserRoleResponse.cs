namespace SimpleVote.Server.Endpoints.Responses;

public record UserRoleResponse(string Id, string UserId, string UserEmail,
    string RoleId, string RoleName);