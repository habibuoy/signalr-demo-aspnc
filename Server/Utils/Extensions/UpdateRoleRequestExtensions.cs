using SimpleVote.Server.Endpoints.Requests;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

public static class UpdateRoleRequestExtensions
{
    public static Role ToRole(this UpdateRoleRequest request, string roleId)
    {
        var role = Role.Create(request.Name, request.Description);
        role.Id = roleId;

        return role;
    }
}