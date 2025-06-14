using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(string id);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<User?> CreateUserAsync(User user);
    Task<bool> AuthenticateAsync(User user, string password);
}