using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

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

        while (!stoppingToken.IsCancellationRequested)
        {
            var vote = await notificationReader.ReadAsync();
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

            if (count < MaxReadCount
                && updateWaitCounters.GetOrAdd(voteId, 0) > 0) continue;

            using var scope = serviceProvider.CreateScope();

            var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

            var v = await voteService.GetVoteByIdAsync(voteId);
            updateCounts[voteId] = 0;
            updateWaitCounters[voteId] += 1;

            if (v == null) continue;

            var voteHubContext = serviceProvider.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
            await voteHubContext.Clients.Group(VoteHub.GetVoteGroupName(voteId))
                .NotifyVoteUpdated(v.ToVoteUpdatedProperties());
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}