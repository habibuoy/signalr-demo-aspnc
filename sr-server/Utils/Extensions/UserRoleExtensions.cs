using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class UserRoleExtensions
{
    public static UserRoleDto ToDto(this UserRole userRole)
    {
        return new UserRoleDto
        {
            Id = userRole.Id,
            UserId = userRole.UserId,
            UserEmail = userRole.User?.Email ?? string.Empty,
            RoleId = userRole.RoleId,
            RoleName = userRole.Role?.Name ?? string.Empty
        };
    }

    public static RoleDto ToRoleDto(this UserRole userRole)
    {
        return new RoleDto
        {
            Id = userRole.Role?.Id ?? string.Empty,
            Name = userRole.Role?.Name ?? string.Empty,
            NormalizedName = userRole.Role?.NormalizedName ?? string.Empty,
            Description = userRole.Role?.Description,
            CreatedTime = userRole.Role?.CreatedTime ?? DateTime.MinValue
        };
    }

    public static UserDto ToUserDto(this UserRole userRole)
    {
        return new UserDto
        {
            Id = userRole.User?.Id ?? string.Empty,
            Email = userRole.User?.Email ?? string.Empty,
            FirstName = userRole.User?.FirstName ?? string.Empty,
            LastName = userRole.User?.LastName ?? string.Empty
        };
    }
}