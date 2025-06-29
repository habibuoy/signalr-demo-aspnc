using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using static SignalRDemo.Server.Utils.LogHelper;

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

        Role? result = null;

        var existing = await GetRoleByNameAsync(name);
        if (existing != null)
        {
            LogWarning(logger, $"Trying to create role with name {name} that already exists.");
            return result;
        }

        try
        {
            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();
            result = role;

            LogInformation(logger, $"Successfully created role {name}.");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB Error happened while creating role {name}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected Error happened while creating role {name}",
                ex);
        }

        return result;
    }

    public async Task<bool> UpdateRoleAsync(Role role)
    {
        var existing = await GetRoleByIdAsync(role.Id);

        bool result = false;

        if (existing == null)
        {
            LogWarning(logger, $"Trying to update role with ID {role.Id} that does not exist.");
            return result;
        }

        existing.Name = role.Name;
        existing.NormalizedName = role.NormalizedName;
        existing.Description = role.Description;

        try
        {
            dbContext.Roles.Update(existing);
            await dbContext.SaveChangesAsync();
            result = true;

            LogInformation(logger, $"Successfully updated role {role.Name}.");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB Error happened while updating role {role.Name}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected Error happened while updating role {role.Name}",
                ex);
        }

        return result;
    }

    public async Task<bool> DeleteRoleAsync(Role role)
    {
        bool result = false;

        try
        {
            dbContext.Roles.Remove(role);
            await dbContext.SaveChangesAsync();
            result = true;

            LogInformation(logger, $"Role {role.Name} deleted successfully.");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB Error happened while deleting role {role.Name}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected Error happened while deleting role {role.Name}",
                ex);
        }

        return result;
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
        UserRole? result = null;
        if (user == null || role == null)
        {
            return result;
        }

        try
        {
            var userRole = UserRole.Create(user.Id, role.Id);
            dbContext.UserRoles.Add(userRole);
            await dbContext.SaveChangesAsync();

            result = userRole;

            LogInformation(logger, $"User {user.Email} ({user.Id}) assigned to " +
                $"role {role.Name} ({role.Id}) successfully.");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB Error happened while assigning role {role.Name} ({role.Id}) " +
                $"to user {user.Email} ({user.Id}): {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while assigning role {role.Name} ({role.Id}) " +
                $"to user {user.Email} ({user.Id})",
                ex);
        }

        return result;
    }

    public async Task<bool> RemoveUserFromRoleAsync(User user, Role role)
    {
        bool result = false;
        if (user == null || role == null)
        {
            return result;
        }

        var userRole = await GetUserRoleAsync(user, role);
        if (userRole == null)
        {
            LogWarning(logger, $"Trying to remove role {role.Name} ({role.Id}) from user {user.Email} ({user.Id}) " +
                "who does not have that role");
            return result;
        }

        try
        {
            dbContext.UserRoles.Remove(userRole);
            await dbContext.SaveChangesAsync();

            result = true;

            LogInformation(logger, $"User {user.Email} ({user.Id}) removed from " +
                $"role {role.Name} ({role.Id}) successfully.");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB Error happened while removing role {role.Name} ({role.Id}) " +
                $"from user {user.Email} ({user.Id}): {ex.Message}.");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while assigning role {role.Name} ({role.Id}) " +
                $"to user {user.Email} ({user.Id})",
                ex);
        }

        return result;
    }
}