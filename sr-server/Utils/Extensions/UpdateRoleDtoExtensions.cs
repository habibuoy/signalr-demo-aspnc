using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class UpdateRoleDtoExtensions
{
    public static Role ToRole(this UpdateRoleDto dto, string roleId)
    {
        var role = Role.Create(dto.Name, dto.Description);
        role.Id = roleId;

        return role;
    }
}