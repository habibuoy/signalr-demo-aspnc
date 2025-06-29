using SimpleVote.Server.Endpoints.Responses;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

public static class RoleExtensions
{
    public static RoleResponse ToResponse(this Role role)
    {
        return new(role.Id,
            role.Name,
            role.NormalizedName,
            role.Description,
            role.CreatedTime);
    }
}