using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Interfaces;

public interface IRoleService
{
    Task<Role?> GetRoleByIdAsync(string id);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> CreateRoleAsync(Role role);
    Task<bool> DeleteRoleAsync(Role role);
    Task<bool> UpdateRoleAsync(Role role);
    Task<UserRole?> AssignUserToRoleAsync(User user, Role role);
    Task<bool> RemoveUserFromRoleAsync(User user, Role role);
    Task<UserRole?> GetUserRoleByIdAsync(string id);
    Task<UserRole?> GetUserRoleAsync(User user, Role role);
    Task<IEnumerable<UserRole>> GetUserRolesByUserAsync(User user);
    Task<IEnumerable<UserRole>> GetUserRolesByRoleAsync(Role role);
}