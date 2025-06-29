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

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger(nameof(UserConfigurations));

        var user = await userServices.GetUserByEmailAsync(CreatorEmail);
        if (user == null)
        {
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
                LogHelper.LogInformation(logger, $"Succesfully created Creator User ({CreatorEmail}) " +
                    $"with password {CreatorPassword}");
            }
        }

        var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
        var adminRole = await roleService.GetRoleByNameAsync(AppConstants.AdminRoleName);
        if (adminRole == null)
        {
            LogHelper.LogInformation(logger, $"Role {AppConstants.AdminRoleName} does not exist yet. Creating...");
            try
            {
                adminRole = await roleService.CreateRoleAsync(Role.Create(AppConstants.AdminRoleName,
                    "Administrator role with all permissions"));
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
                LogHelper.LogError(logger, $"Unexpected error happened while " +
                    $"creating role {AppConstants.AdminRoleName}", ex);
            }

            if (adminRole == null)
            {
                LogHelper.LogCritical(logger, $"Failed creating role {AppConstants.AdminRoleName}");
            }
            else
            {
                LogHelper.LogInformation(logger, $"Succesfully created role {AppConstants.AdminRoleName}");
            }
        }

        if (user != null && adminRole != null)
        {
            var creatorAdminRole = await roleService.GetUserRoleAsync(user, adminRole);
            if (creatorAdminRole == null)
            {
                LogHelper.LogInformation(logger, $"Creator user {user.Email} " +
                    $"does not have role {AppConstants.AdminRoleName} yet. Assigning...");

                try
                {
                    creatorAdminRole = await roleService.AssignUserToRoleAsync(user, adminRole);
                }
                catch (Exception ex)
                {
                    LogHelper.LogError(logger, $"Unexpected error happened while assigning " +
                        $"role {AppConstants.AdminRoleName} to Creator user {user.Email}", ex);
                }

                if (adminRole == null)
                {
                    LogHelper.LogCritical(logger, $"Failed assigning role {AppConstants.AdminRoleName}" +
                        $"to Creator user {user.Email}");
                }
                else
                {
                    LogHelper.LogInformation(logger, $"Succesfully assigned role {AppConstants.AdminRoleName}" +
                        $"to Creator user {user.Email}");
                }
            }
        }

        return webApplication;
    }
}