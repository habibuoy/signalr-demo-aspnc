using SignalRDemo.Server.Endpoints.Responses;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

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