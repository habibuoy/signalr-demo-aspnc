using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateUserDtoExtensions
{
    public static User ToUser(this CreateUserDto dto)
    {
        return User.Create(dto.Email, dto.Password, dto.FirstName, dto.LastName);
    }
}