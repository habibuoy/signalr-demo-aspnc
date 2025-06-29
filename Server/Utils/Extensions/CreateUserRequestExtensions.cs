using SignalRDemo.Server.Endpoints.Requests;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateUserRequestExtensions
{
    public static User ToUser(this CreateUserRequest request)
    {
        return User.Create(request.Email,
            request.Password,
            request.FirstName,
            request.LastName);
    }
}