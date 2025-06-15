using System.Data;
using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Utils;
using static SignalRDemo.Server.Utils.LogHelper;

namespace SignalRDemo.Server.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext dbContext;
    private readonly ILogger<UserService> logger;

    public UserService(ApplicationDbContext dbContext, ILogger<UserService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public Task<User?> GetUserByEmailAsync(string email) =>
        dbContext.Users.FirstOrDefaultAsync(u => email != null && u.Email == email);

    public Task<User?> GetUserByIdAsync(string id) =>
        dbContext.Users.FirstOrDefaultAsync(u => id != null && u.Id == id);

    public async Task<IEnumerable<User>> GetAllUsersAsync() =>
        await dbContext.Users.ToListAsync();

    public Task<bool> AuthenticateAsync(User user, string password)
    {
        if (user == null
            || password == null
            || password.Length == 0)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(PasswordHasher.VerifyHash(user.PasswordHash, password));
    }

    public async Task<User?> CreateUserAsync(User user)
    {
        var email = user.Email;
        User? result = null;

        try
        {
            dbContext.Add(user);
            await dbContext.SaveChangesAsync();

            result = user;

            LogInformation(logger, $"Successfully created user {user.Email} ({user.Id})");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while adding new user {email}: {ex.Message}");
        }
        catch (DBConcurrencyException ex)
        {
            LogWarning(logger, "DB Concurrency error happened while " +
                $"adding new user {email}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Operation cancelled while adding new user {email}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unknown error happened while adding new user {email}",
                ex);
        }

        return result;
    }
}
