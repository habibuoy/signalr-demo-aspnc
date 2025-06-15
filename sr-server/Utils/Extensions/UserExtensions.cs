using SignalRDemo.Server.Endpoints.Responses;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class UserExtensions
{
    public static UserResponse ToResponse(this User user)
    {
        return new(user.Id,
            user.Email,
            user.FirstName,
            user.LastName);
    }
}