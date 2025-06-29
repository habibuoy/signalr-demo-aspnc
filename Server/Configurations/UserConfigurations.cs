using SimpleVote.Server.Interfaces;
using SimpleVote.Server.Models;
using SimpleVote.Server.Utils;

namespace SimpleVote.Server.Configurations;

public static class UserConfigurations
{
    private const string CreatorEmail = "creator@gmail.com";
    private const string CreatorPassword = "Password123@";

    public static async Task<WebApplication> ConfigureUsers(this WebApplication webApplication)
    {
        using var scope = webApplication.Services.CreateScope();
        var userServices = scope.ServiceProvider.GetRequiredService<IUserService>();

        var user = await userServices.GetUserByEmailAsync(CreatorEmail);
        if (user == null)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(UserConfigurations));
            LogHelper.LogInformation(logger, $"Creator user ({CreatorEmail}) does not exist yet. Creating...");

            try
            {
                user = await userServices.CreateUserAsync(User.Create(CreatorEmail, CreatorPassword, "The", "Creator"));
            }
            catch (DomainValidationException ex)
            {
                LogHelper.LogError(logger, $"Validation error: {string.Join(", ",
                    ex.ValidationErrors.Values.Aggregate(new List<string>(), (acc, elm) =>
                    {
                        acc.AddRange(elm);
                        return acc;
                    }))}");
            }
            catch (Exception ex)
            {
                LogHelper.LogError(logger, "Unexpected error happened while creating Creator User", ex);
            }

            if (user == null)
            {
                LogHelper.LogCritical(logger, "Failed creating creator user");
            }
            else
            {
                LogHelper.LogInformation(logger, $"Succesfully created Creator User ({CreatorEmail}) with password {CreatorPassword}");
            }
        }

        return webApplication;
    }
}