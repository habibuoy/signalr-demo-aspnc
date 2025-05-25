using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Models;

public class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    // public required byte[] PasswordHash { get; set; }
    // public required byte[] PasswordSalt { get; set; }
    public required string PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public static class UserExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }
}