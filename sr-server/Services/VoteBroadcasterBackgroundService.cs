using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Utils.Extensions;
using SignalRDemo.Server.SignalRHubs;

namespace SignalRDemo.Server.Services;

public class VoteBroadcasterBackgroundService : BackgroundService
{
    private const int MaxReadCount = 10;

    private readonly IVoteNotificationReader notificationReader;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<VoteBroadcasterBackgroundService> logger;
    private readonly Dictionary<string, int> updateCounts = new();
    private readonly ConcurrentDictionary<string, int> updateWaitCounters = new();

    public VoteBroadcasterBackgroundService(IVoteNotificationReader notificationReader,
        IServiceProvider serviceProvider,
        ILogger<VoteBroadcasterBackgroundService> logger)
    {
        this.notificationReader = notificationReader;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _ = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var waitCounter in updateWaitCounters)
                {
                    var id = waitCounter.Key;
                    var value = waitCounter.Value;

                    if (value <= 0) continue;

                    updateWaitCounters[id] = Math.Min(0, value - 1);
                }

                await Task.Delay(1000);
            }
        }, CancellationToken.None);

        var readCreatedVoteNotificationsTask = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var vote = await notificationReader.ReadCreatedNotificationAsync();
                if (vote == null)
                {
                    continue;
                }

                updateWaitCounters.TryAdd(vote.Id, 0);

                try
                {
                    var voteHubContext = serviceProvider.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
                    await voteHubContext.Clients.All.NotifyVoteCreated(vote.ToVoteCreatedProperties());
                }
                catch (InvalidOperationException ex)
                {
                    logger.LogInformation("Error while hubbing: {msg}", ex.Message);
                }
            }
        }, CancellationToken.None);

        var readUpdatedVoteNotificationsTask = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var vote = await notificationReader.ReadUpdatedNotificationAsync();
                if (vote == null)
                {
                    continue;
                }

                string voteId = vote.Id;

                if (!updateCounts.TryGetValue(voteId, out var count))
                {
                    updateCounts.Add(voteId, 0);
                }

                count = ++updateCounts[voteId];
                var updateWaitCounter = updateWaitCounters.GetOrAdd(voteId, 0);
                logger.LogInformation("update wait counter of vote {vote.Title} is {updateWaitCounter}",
                    vote.Title, updateWaitCounter);

                if (count < MaxReadCount
                    && updateWaitCounter > 0) continue;

                using var scope = serviceProvider.CreateScope();

                var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

                var v = await voteService.GetVoteByIdAsync(voteId);
                updateCounts[voteId] = 0;
                updateWaitCounters[voteId] += 1;

                if (v == null) continue;

                logger.LogInformation("notifying {vote.Title}", vote.Title);
                var voteHubContext = serviceProvider.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
                await voteHubContext.Clients.Group(VoteHub.GetVoteGroupName(voteId))
                    .NotifyVoteUpdated(v.ToVoteUpdatedProperties());
            }
        }, CancellationToken.None);

        await Task.WhenAll(readCreatedVoteNotificationsTask, readUpdatedVoteNotificationsTask);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}