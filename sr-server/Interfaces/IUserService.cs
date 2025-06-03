using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(string id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> CreateUserAsync(string email, string password,
        string? firstName, string? lastName);
    Task<bool> AuthenticateAsync(User user, string password);
}