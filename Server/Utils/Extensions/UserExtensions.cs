using SimpleVote.Server.Endpoints.Responses;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

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