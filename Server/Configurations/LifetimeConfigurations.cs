using SimpleVote.Server.Interfaces;

namespace SimpleVote.Server.Configurations;

public static class LifetimeConfigurations
{
    public static IHostApplicationLifetime ConfigureLifetime(this IHostApplicationLifetime lifetime,
        IServiceProvider serviceProvider)
    {
        lifetime.ApplicationStopping.Register(async () =>
        {
            var voteQueue = serviceProvider.GetRequiredService<IVoteQueueWriter>();
            var voteNotification = serviceProvider.GetRequiredService<IVoteNotificationWriter>();

            await voteQueue.CloseAsync();
            await voteNotification.CloseAsync();
        });

        return lifetime;
    }
}