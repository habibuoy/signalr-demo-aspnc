using SimpleVote.Server.Endpoints.Requests;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

public static class CreateRoleRequestExtensions
{
    public static Role ToRole(this CreateRoleRequest request)
    {
        return Role.Create(request.Name,
            request.Description);
    }
}