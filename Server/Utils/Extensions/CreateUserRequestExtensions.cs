using SimpleVote.Server.Endpoints.Requests;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

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