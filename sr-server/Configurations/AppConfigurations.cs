using Microsoft.EntityFrameworkCore.Diagnostics;
using SignalRDemo.Server.Datas;
using static SignalRDemo.Server.Configurations.AppConstants;

namespace SignalRDemo.Server.Configurations;

public static class AppConfigurations
{
    public static IServiceCollection AddGeneralServices(this IServiceCollection services)
    {
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
        services.AddSignalR();

        return services;
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString(MainDbName);

        services.AddSqlite<ApplicationDbContext>(connectionString,
            optionsAction: options =>
            {
                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.ConfigureWarnings(w =>
                    {
                        w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
                    });
                }
            }
        );

        return services;
    }
}