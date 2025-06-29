using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SimpleVote.Server.Interfaces;
using SimpleVote.Server.Utils.Extensions;
using SimpleVote.Server.SignalRHubs;
using static SimpleVote.Server.Utils.LogHelper;

namespace SimpleVote.Server.Services;

public class VoteBroadcasterBackgroundService : BackgroundService
{
    // Number of updated vote being held before a Vote Updated notification
    // is sent to all vote update subscribers.
    private const int MaxUpdateHoldCount = 1;

    // Number of time in seconds where a Vote's Update notification will be sent
    // regardless of its UpdateHoldCount has reached the max value or not.
    // For example, if current update count of vote id xxx is 3 with maximum
    // update hold count of 10, and the wait time of that Vote has reached 0, then the notification
    // will be sent, and both wait time and hold count will be reset.
    private const int UpdateWaitTime = 1;

    // Number of time in milliseconds of when the votes that are being held in the update hold count
    // and update time counters dictionaries will be checked and removed if necessary
    // so the dictionaries are always free from unnecessary votes (expired or exceeded maximum count)
    private const int HeldVotesRemovalCheckInterval = 5000;

    private readonly IVoteNotificationReader notificationReader;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<VoteBroadcasterBackgroundService> logger;
    private readonly ConcurrentDictionary<string, int> updateHoldCounts = new();
    private readonly ConcurrentDictionary<string, int> updateWaitTimeCounters = new();

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
        var waitCountersUpdaterTask = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var waitCounter in updateWaitTimeCounters)
                {
                    var id = waitCounter.Key;
                    var value = waitCounter.Value;

                    if (value <= 0) continue;

                    updateWaitTimeCounters[id] = Math.Min(0, value - 1);
                }

                await Task.Delay(1000);
            }
        }, CancellationToken.None);

        var heldVotesRemoverTask = Task.Run(async () =>
        {
            await Task.Delay(HeldVotesRemovalCheckInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var voteId in updateHoldCounts.Keys)
                {
                    var scope = serviceProvider.CreateScope();
                    var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

                    var vote = await voteService.GetVoteByIdAsync(voteId);
                    if (vote == null)
                    {
                        RemoveVoteFromDictionaries(voteId);
                        continue;
                    }

                    if (vote.CanVote())
                    {
                        continue;
                    }

                    RemoveVoteFromDictionaries(voteId);
                }

                await Task.Delay(HeldVotesRemovalCheckInterval);
            }
        });

        var readCreatedVoteNotificationsTask = Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var vote = await notificationReader.ReadCreatedNotificationAsync();
                if (vote == null)
                {
                    continue;
                }

                updateWaitTimeCounters.TryAdd(vote.Id, 0);

                try
                {
                    var voteHubContext = serviceProvider.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
                    await voteHubContext.Clients.All.NotifyVoteCreated(vote.ToVoteCreatedProperties());
                }
                catch (InvalidOperationException ex)
                {
                    LogWarning(logger, $"Operation error happened while notifying vote created: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogError(logger, $"Unexpected error happened while notifying vote created",
                        ex);
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

                var count = updateHoldCounts.GetOrAdd(voteId, 0) + 1;
                var updateWaitCounter = updateWaitTimeCounters.GetOrAdd(voteId, 0);

                if (count < MaxUpdateHoldCount
                    && updateWaitCounter > 0) continue;

                using var scope = serviceProvider.CreateScope();

                var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

                var v = await voteService.GetVoteByIdAsync(voteId);
                updateHoldCounts[voteId] = 0;
                updateWaitTimeCounters[voteId] = UpdateWaitTime;

                if (v == null) continue;

                try
                {
                    var voteHubContext = serviceProvider.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
                    await voteHubContext.Clients.Group(VoteHub.GetVoteGroupName(voteId))
                        .NotifyVoteUpdated(v.ToVoteUpdatedProperties());
                }
                catch (InvalidOperationException ex)
                {
                    LogWarning(logger, $"Operation error happened while notifying vote updated: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogError(logger, $"Unexpected error happened while notifying vote updated",
                        ex);
                }

                if (!vote.CanVote())
                {
                    RemoveVoteFromDictionaries(voteId);
                }
            }
        }, CancellationToken.None);

        await Task.WhenAll(
            waitCountersUpdaterTask,
            readCreatedVoteNotificationsTask,
            readUpdatedVoteNotificationsTask);
    }

    private void RemoveVoteFromDictionaries(string voteId)
    {
        updateHoldCounts.Remove(voteId, out _);
        updateWaitTimeCounters.Remove(voteId, out _);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return base.StopAsync(cancellationToken);
    }
}