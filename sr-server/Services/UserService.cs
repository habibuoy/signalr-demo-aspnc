using System.Data;
using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Utils;

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

    public async Task<User?> FindUserByEmailAsync(string email)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public Task<bool> AuthenticateAsync(User user, string password)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(password);

        return Task.FromResult(PasswordHasher.VerifyHash(user.PasswordHash, password));
    }

    public async Task<User?> CreateUserAsync(string email, string password, string? firstName, string? lastName)
    {
        ArgumentException.ThrowIfNullOrEmpty(email);
        ArgumentException.ThrowIfNullOrEmpty(password);

        var user = new User()
        {
            Id = Guid.NewGuid().ToString(),
            Email = email,
            FirstName = firstName,
            PasswordHash = PasswordHasher.Hash(password),
            LastName = lastName,
            CreatedTime = DateTime.UtcNow
        };

        dbContext.Add(user);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case DbUpdateException:
                    logger.LogInformation($"({nameof(CreateUserAsync)}): " +
                        $"DB error happened while adding new user {email}");
                    break;
                case DBConcurrencyException:
                    logger.LogInformation($"({nameof(CreateUserAsync)}): " +
                        $"DB Concurrency error happened while adding new user {email}");
                    break;
                case OperationCanceledException:
                    logger.LogInformation($"({nameof(CreateUserAsync)}): " +
                        $"Operation cancelled while adding new user {email}");
                    break;
                default:
                    logger.LogInformation(ex, $"({nameof(CreateUserAsync)}): " +
                        $"Unknown error happened while adding new user {email}");
                    break;
            }

            return null;
        }

        return user;
    }
}
