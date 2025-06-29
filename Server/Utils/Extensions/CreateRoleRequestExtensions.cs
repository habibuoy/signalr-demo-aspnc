using SignalRDemo.Server.Endpoints.Requests;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateRoleRequestExtensions
{
    public static Role ToRole(this CreateRoleRequest request)
    {
        return Role.Create(request.Name,
            request.Description);
    }
}