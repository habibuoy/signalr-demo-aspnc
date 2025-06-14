using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext dbContext;
    private readonly ILogger<RoleService> logger;

    public RoleService(ApplicationDbContext dbContext, ILogger<RoleService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public Task<Role?> GetRoleByIdAsync(string id) =>
        dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);

    public Task<Role?> GetRoleByNameAsync(string name) =>
        dbContext.Roles.FirstOrDefaultAsync(r => r.Name.Equals(name.ToLower()));

    public async Task<IEnumerable<Role>> GetAllRolesAsync() =>
        await dbContext.Roles.ToListAsync();

    public async Task<Role?> CreateRoleAsync(Role role)
    {
        var name = role.Name;

        var existing = await GetRoleByNameAsync(name);
        if (existing != null)
        {
            logger.LogWarning("Trying to create role with name {name} that already exists.", name);
            return null;
        }

        dbContext.Roles.Add(role);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Role {name} created successfully.", name);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DB Error happened while creating role {name}.", name);
            return null;
        }

        return role;
    }

    public async Task<bool> UpdateRoleAsync(Role role)
    {
        var existing = await GetRoleByIdAsync(role.Id);
        if (existing == null)
        {
            logger.LogWarning("Trying to update role with ID {id} that does not exist.", role.Id);
            return false;
        }

        existing.Name = role.Name;
        existing.NormalizedName = role.NormalizedName;
        existing.Description = role.Description;

        dbContext.Roles.Update(existing);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Role {name} updated successfully.", role.Name);
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DB Error happened while updating role {name}.", role.Name);
            return false;
        }
    }

    public async Task<bool> DeleteRoleAsync(Role role)
    {
        dbContext.Roles.Remove(role);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Role {name} deleted successfully.", role.Name);
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DB Error happened while deleting role {name}.", role.Name);
            return false;
        }
    }

    public Task<UserRole?> GetUserRoleAsync(User user, Role role) =>
        dbContext.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ur => user != null && ur.UserId == user.Id && role != null && ur.RoleId == role.Id);

    public Task<UserRole?> GetUserRoleByIdAsync(string id) =>
        dbContext.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .AsSplitQuery()
            .FirstOrDefaultAsync(ur => ur.Id == id);

    public async Task<IEnumerable<UserRole>> GetUserRolesByRoleAsync(Role role) =>
        await dbContext.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .AsSplitQuery()
            .Where(ur => role != null && ur.RoleId == role.Id).ToListAsync();

    public async Task<IEnumerable<UserRole>> GetUserRolesByUserAsync(User user) =>
        await dbContext.UserRoles
            .Include(ur => ur.User)
            .Include(ur => ur.Role)
            .AsSplitQuery()
            .Where(ur => user != null && ur.UserId == user.Id).ToListAsync();

    public async Task<UserRole?> AssignUserToRoleAsync(User user, Role role)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(role);

        var userRole = UserRole.Create(user.Id, role.Id);

        dbContext.UserRoles.Add(userRole);

        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("User {email} ({userId}) assigned to role {roleName} ({roleId}) successfully.",
                user.Email, user.Id, role.Name, role.Id);
            return userRole;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DB Error happened while assigning role {roleName} ({roleId}) from user {email} ({userId}).",
                role.Name, role.Id, user.Email, user.Id);
            return null;
        }
    }

    public async Task<bool> RemoveUserFromRoleAsync(User user, Role role)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(role);

        var userRole = await GetUserRoleAsync(user, role);
        if (userRole == null)
        {
            logger.LogWarning("Trying to remove role {roleName} ({roleId}) from user {email} ({userId}) which does not have that role",
                role.Name, role.Id, user.Email, user.Id);
            return false;
        }

        dbContext.UserRoles.Remove(userRole);
        try
        {
            await dbContext.SaveChangesAsync();
            logger.LogInformation("User {email} ({userId}) removed from role {roleName} ({roleId}) successfully.",
                user.Email, user.Id, role.Name, role.Id);
            return true;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "DB Error happened while removing role {roleName} ({roleId}) from user {email} ({userId}).",
                role.Name, role.Id, user.Email, user.Id);
            return false;
        }
    }
}