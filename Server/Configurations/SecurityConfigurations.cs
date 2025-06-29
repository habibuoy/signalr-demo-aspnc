using SignalRDemo.Server.Endpoints.Handlers;
using static SignalRDemo.Server.Configurations.AppConstants;

namespace SignalRDemo.Server.Configurations;

public static class SecurityConfigurations
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        services.AddAuthentication()
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                options.AccessDeniedPath = "/" + nameof(RootHandlers.AccessDenied);
                options.LoginPath = "/" + nameof(RootHandlers.AccessDenied);
                options.SlidingExpiration = true;
                options.Cookie.Name = AuthCookieName;
                // TODO: Change below properties back to secure ones if the web client is moved to same project
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.HttpOnly = false;
                options.Cookie.MaxAge = options.ExpireTimeSpan;
            }
        );

        services.AddAuthorization(configure =>
        {
            configure.AddPolicy(RoleManagerAuthorizationPolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AdminRoleName);
            });

            configure.AddPolicy(VoteAdministratorAuthorizationPolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AdminRoleName, VoteAdminRoleName);
            });

            configure.AddPolicy(VoteInspectorAuthorizationPolicyName, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AdminRoleName, VoteInspectorRoleName);
            });
        });

        return services;
    }
}