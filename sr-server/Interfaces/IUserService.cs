using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IUserService
{
    Task<User?> FindUserByEmailAsync(string email);
    Task<User?> CreateUserAsync(string email, string password,
        string? firstName, string? lastName);
    Task<bool> AuthenticateAsync(User user, string password);
}