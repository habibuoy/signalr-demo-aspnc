using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Services;

public class DbVoteService : IVoteService
{
    private readonly ApplicationDbContext dbContext;
    private readonly ILogger<DbVoteService> logger;

    public DbVoteService(ApplicationDbContext dbContext,
        ILogger<DbVoteService> logger)
    {
        this.dbContext = dbContext;
        this.logger = logger;
    }

    public async Task<bool> AddVoteAsync(Vote vote)
    {
        try
        {
            // clean the ids, let the EF Core generate them
            foreach (var subject in vote.Subjects)
            {
                subject.Id = 0;
                subject.VoteCount.Id = 0;
                subject.VoteCount.SubjectId = 0;
            }

            dbContext.Add(vote);

            await dbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case DbUpdateException or DbUpdateConcurrencyException:
                    logger.LogInformation("Db error happened while trying to add vote {id}: {msg}",
                        vote.Id, ex.Message);
                    break;
                case OperationCanceledException:
                    logger.LogInformation("Task cancelled while trying to add vote {id}: {msg}",
                        vote.Id, ex.Message);
                    break;
                default:
                    logger.LogError(ex, "Unexpected error happened while trying to add vote {id}: {msg}",
                        vote.Id, ex.Message);
                    break;
            }

            return false;
        }
    }

    public async Task<Vote?> GetVoteByIdAsync(string id)
    {
        // Use FirstOrDefaultAsync instead of FindAsync,
        // because we need the Queryable.Include and .ThenInclude.
        // FindAsync does not include the relational properties.
        var vote = await dbContext.Votes
            .Include(v => v.Subjects)
            .ThenInclude(s => s.VoteCount)
            .FirstOrDefaultAsync(v => v.Id == id);
        logger.LogInformation("subjects count: {c}", vote!.Subjects.Count);
        return vote;
    }

    public async Task<bool> RemoveVoteAsync(string id)
    {
        try
        {
            var vote = await GetVoteByIdAsync(id);
            if (vote != null)
            {
                dbContext.Votes.Remove(vote);
                await dbContext.SaveChangesAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case DbUpdateException or DbUpdateConcurrencyException:
                    logger.LogInformation("Db error happened while trying to remove vote {id}: {msg}",
                        id, ex.Message);
                    break;
                case OperationCanceledException:
                    logger.LogInformation("Task cancelled while trying to remove vote {id}: {msg}",
                        id, ex.Message);
                    break;
            }

            return false;
        }
    }

    public async Task<bool> UpdateVoteAsync(string voteId, Vote vote)
    {
        try
        {
            var existing = await dbContext.Votes.FindAsync(voteId);
            if (existing != null)
            {
                existing.Subjects = vote.Subjects;
                existing.Title = vote.Title;
                existing.Subjects = vote.Subjects;
                await dbContext.SaveChangesAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case DbUpdateConcurrencyException:
                    logger.LogInformation("Db concurrency error happened while trying to update vote {id}: {msg}",
                        voteId, ex.Message);
                    var conEx = ex as DbUpdateConcurrencyException;
                    foreach (var entry in conEx!.Entries)
                    {
                        if (entry.Entity is Vote)
                        {
                            var proposedValues = entry.CurrentValues;
                            var databaseValues = await entry.GetDatabaseValuesAsync();
                            foreach (var property in proposedValues.Properties)
                            {
                                var databaseValue = databaseValues![property];
                                // revert value to database value
                                proposedValues[property] = databaseValue;
                            }

                            entry.OriginalValues.SetValues(databaseValues!);
                        }
                    }
                    throw;
                case DbUpdateException:
                    logger.LogInformation("Db error happened while trying to update vote {id}: {msg}",
                        voteId, ex.Message);
                    break;
                case OperationCanceledException:
                    logger.LogInformation("Task cancelled while trying to add remove {id}: {msg}",
                        voteId, ex.Message);
                    break;
            }

            return false;
        }
    }
}