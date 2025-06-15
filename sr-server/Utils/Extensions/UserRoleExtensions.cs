using SignalRDemo.Server.Endpoints.Responses;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class UserRoleExtensions
{
    public static UserRoleResponse ToResponse(this UserRole userRole)
    {
        return new(userRole.Id,
            userRole.UserId,
            userRole.User?.Email ?? string.Empty,
            userRole.RoleId, userRole.Role?.Name ?? string.Empty);
    }

    public static RoleResponse ToRoleResponse(this UserRole userRole)
    {
        return new(userRole.Role?.Id ?? string.Empty,
            userRole.Role?.Name ?? string.Empty,
            userRole.Role?.NormalizedName ?? string.Empty,
            userRole.Role?.Description,
            userRole.Role?.CreatedTime ?? DateTime.MinValue);
    }

    public static UserResponse ToUserDto(this UserRole userRole)
    {
        return new(userRole.User?.Id ?? string.Empty,
            userRole.User?.Email ?? string.Empty,
            userRole.User?.FirstName ?? string.Empty,
            userRole.User?.LastName ?? string.Empty);
    }
}