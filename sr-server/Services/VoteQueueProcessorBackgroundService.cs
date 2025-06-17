using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Interfaces;
using static SignalRDemo.Server.Utils.LogHelper;

namespace SignalRDemo.Server.Services;

public class VoteQueueProcessorBackgroundService : BackgroundService
{
    private readonly ILogger<VoteQueueProcessorBackgroundService> logger;
    private readonly IServiceProvider serviceProvider;
    private readonly IVoteQueueReader voteQueueReader;

    public VoteQueueProcessorBackgroundService(ILogger<VoteQueueProcessorBackgroundService> logger,
        IServiceProvider serviceProvider,
        IVoteQueueReader voteQueueReader)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
        this.voteQueueReader = voteQueueReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var item = await voteQueueReader.ReadAsync();
            if (item == default) continue;

            using var scope = serviceProvider.CreateScope();
            var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();
            var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

            var vote = await voteService.GetVoteByIdAsync(item.VoteId);
            var user = await userService.GetUserByIdAsync(item.UserId!);

            if (vote == null)
            {
                LogWarning(logger, $"Vote id {item.VoteId} not found while trying to process vote queue.");
                continue;
            }

            if (user == null)
            {
                LogWarning(logger, $"User id {item.UserId} not found while trying to process vote queue.");
                continue;
            }

            string? email = user.Email;
            var userId = user.Id;

            try
            {
                var result = await voteService.GiveVoteAsync(item.SubjectId, userId);
                if (!result.Succeeded)
                {
                    LogWarning(logger, $"User {email} failed while giving vote on vote id {vote.Id}: " +
                        $"{string.Join(", ", result.Error.Message)}.");
                    continue;
                }

                LogInformation(logger, $"User {email} succeeded giving vote on vote id {vote.Id}.");
            }
            catch (DbUpdateConcurrencyException)
            {
                LogWarning(logger, $"DB Concurrency happened while {user} giving vote for {vote.Id}");
                continue;
            }
            catch (Exception ex)
            {
                LogError(logger, $"Unexpected error happened while {user} giving vote for {vote.Id}",
                    ex);
                continue;
            }

            try
            {
                var voteNotifier = scope.ServiceProvider.GetRequiredService<IVoteNotificationWriter>();
                await voteNotifier.WriteUpdateAsync(vote);
            }
            catch (Exception ex)
            {
                LogError(logger, "Unknown error happened while notifying updated vote",
                    ex);
            }
        }
    }
}
