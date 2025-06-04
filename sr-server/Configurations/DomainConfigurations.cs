using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Services;

namespace SignalRDemo.Server.Configurations;

public static class DomainConfigurations
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IVoteService, DbVoteService>();
        services.AddSingleton<IHubConnectionManager, HubConnectionManager>();
        services.AddSingleton<IHubConnectionReader, HubConnectionManager>();

        var voteQueue = new VoteQueue();
        services.AddSingleton<IVoteQueueReader>(voteQueue);
        services.AddSingleton<IVoteQueueWriter>(voteQueue);

        var voteNotification = new VoteNotification();
        services.AddSingleton<IVoteNotificationReader>(voteNotification);
        services.AddSingleton<IVoteNotificationWriter>(voteNotification);

        services.AddHostedService<VoteQueueProcessorBackgroundService>();
        services.AddHostedService<VoteBroadcasterBackgroundService>();

        return services;
    }
}