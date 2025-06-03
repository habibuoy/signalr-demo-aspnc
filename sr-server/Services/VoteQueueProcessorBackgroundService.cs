using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

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
            var user = await userService.GetUserByIdAsync(item.UserId);

            if (vote == null)
            {
                logger.LogWarning("Vote id {id} not found while trying to process vote queue.", item.VoteId);
                continue;
            }

            if (user == null)
            {
                logger.LogWarning("User id {id} not found while trying to process vote queue.", item.UserId);
                continue;
            }

            string? email = user.Email;
            var userId = user.Id;

            try
            {
                var inputs = vote.Subjects.SelectMany(s => s.Voters);

                if (inputs.Any(i => i.VoterId != null && i.VoterId == userId))
                {
                    logger.LogWarning("User {email} already have given vote on vote id {vote.Id}.", email, vote.Id);
                    continue;
                }

                if (vote.IsClosed()
                    || !vote.CanVote())
                {
                    logger.LogWarning("User {email} failed while giving vote on vote id {vote.Id}. Vote has been closed or exceeded maximum count",
                        email, vote.Id);
                    continue;
                }

                if (vote.Subjects.FirstOrDefault(s => s.Id.ToString() == item.SubjectId) is not VoteSubject subject)
                {
                    logger.LogWarning("User {email} failed while giving vote on vote id {vote.Id}. Subject not found",
                        email, vote.Id);
                    continue;
                }

                var result = await voteService.GiveVoteAsync(subject.Id.ToString(), userId);
                if (!result)
                {
                    logger.LogWarning("User {email} failed while giving vote on vote id {vote.Id}",
                        email, vote.Id);
                }

                logger.LogInformation("User {email} succeeded giving vote on vote id {vote.Id}",
                    email, vote.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogWarning("DB Concurrency happened while {user} giving vote for {vote.Id}", email, vote!.Id);
                continue;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown error happened while {user} giving vote for {vote.Id}", email, vote!.Id);
                continue;
            }

            try
            {
                var voteNotifier = scope.ServiceProvider.GetRequiredService<IVoteNotificationWriter>();
                await voteNotifier.WriteUpdateAsync(vote);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown error happened while notifying updated vote");
            }
        }
    }
}
