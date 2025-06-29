using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SimpleVote.Server.Datas;
using static SimpleVote.Server.Configurations.AppConstants;

namespace SimpleVote.Server.Configurations;

public static class AppConfigurations
{
    public static IServiceCollection AddGeneralServices(this IServiceCollection services)
    {
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();
        services.AddSignalR();
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
        });

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