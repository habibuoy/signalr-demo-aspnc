using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateRoleDtoExtensions
{
    public static Role ToRole(this CreateRoleDto dto)
    {
        return Role.Create(dto.Name, dto.Description);
    }
}