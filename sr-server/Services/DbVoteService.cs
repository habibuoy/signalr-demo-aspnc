using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using static SignalRDemo.Server.Utils.LogHelper;

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
        bool result = false;

        try
        {
            // clean the ids, let the EF Core generate them
            foreach (var subject in vote.Subjects)
            {
                subject.Id = 0;
            }

            dbContext.Add(vote);
            await dbContext.SaveChangesAsync();

            result = true;

            LogInformation(logger, $"Successfully added vote {vote.Title} ({vote.Id})");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while trying to add vote {vote.Id}: {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while trying to add vote {vote.Id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Task cancelled while trying to add vote {vote.Id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while trying to add vote {vote.Id}",
                ex);
        }

        return result;
    }

    public async Task<Vote?> GetVoteByIdAsync(string id)
    {
        // Use FirstOrDefaultAsync instead of FindAsync,
        // because we need the Queryable.Include and .ThenInclude.
        // FindAsync does not include the relational properties.
        var vote = await dbContext.Votes
            .Include(v => v.Subjects)
            .ThenInclude(s => s.Voters)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == id);
        return vote;
    }

    public async Task<IEnumerable<Vote>> GetVotesAsync(int? count = 10,
        string? sortBy = null,
        string? sortOrder = null,
        Func<Vote, bool>? predicate = null)
    {
        var votes = dbContext.Votes.AsQueryable();

        if (predicate != null)
        {
            votes = votes.Where(predicate).AsQueryable();
        }

        bool sortDesc = sortOrder != null && sortOrder == "desc";

        if (!string.IsNullOrEmpty(sortBy))
        {
            if (sortBy == "cdt")
            {
                if (sortDesc)
                {
                    votes = votes.OrderByDescending(v => v.CreatedTime);
                }
                else
                {
                    votes = votes.OrderBy(v => v.CreatedTime);
                }
            }
        }
        else
        {
            if (sortDesc)
            {
                votes = votes.OrderByDescending(v => v.CreatedTime);
            }
            else
            {
                votes = votes.OrderBy(v => v.CreatedTime);
            }
        }

        votes = votes.Take(count == null ? 10 : count!.Value);

        votes = votes
            .Include(v => v.Subjects)
            .ThenInclude(s => s.Voters)
            .AsSplitQuery();

        return await votes.ToListAsync();
    }

    public async Task<IEnumerable<VoteSubjectInput>> GetVoteInputsByUserIdAsync(string userId)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);

        var voteInputs = dbContext.VoteSubjectInputs
            .Where(si => si.VoterId == userId)
            .Include(si => si.VoteSubject)
            .ThenInclude(vs => vs!.Vote)
            .AsSplitQuery();

        return await voteInputs.ToListAsync();
    }

    public async Task<IEnumerable<VoteSubjectInput>?> GetVoteInputsByVoteIdAsync(string voteId)
    {
        ArgumentException.ThrowIfNullOrEmpty(voteId);

        var vote = await dbContext.Votes
            .Include(v => v.Subjects)
            .ThenInclude(s => s.Voters)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Id == voteId);

        if (vote == null) return null;

        return vote.Subjects.SelectMany(s => s.Voters);
    }

    public async Task<bool> RemoveVoteAsync(string id)
    {
        bool result = false;

        try
        {
            var vote = await GetVoteByIdAsync(id);
            if (vote != null)
            {
                dbContext.Votes.Remove(vote);
                await dbContext.SaveChangesAsync();

                result = true;

                LogInformation(logger, $"Successfully removed vote {vote.Title} ({vote.Id})");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while trying to remove vote {id}: {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while trying to remove vote {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Task cancelled while trying to remove vote {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while trying to remove vote {id}",
                ex);
        }

        return result;
    }

    public async Task<bool> UpdateVoteAsync(string voteId, Vote vote)
    {
        bool result = false;
        string id = vote.Id;

        try
        {
            var existing = await dbContext.Votes.FindAsync(voteId);
            if (existing != null)
            {
                existing.Subjects = vote.Subjects;
                existing.Title = vote.Title;
                await dbContext.SaveChangesAsync();

                result = true;

                LogInformation(logger, $"Successfully updated vote {vote.Title} ({id})");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while trying to " +
                $"update vote {id}: {ex.Message}. Reverting changes to DB values");
            foreach (var entry in ex.Entries)
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
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while trying to update vote {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Task cancelled while trying to update vote {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while trying to update vote {id}",
                ex);
        }

        return result;
    }

    public async Task<bool> GiveVoteAsync(string subjectId, string? userId)
    {
        bool result = false;

        try
        {
            var subject = await dbContext.VoteSubjects
                .Include(vs => vs.Vote)
                .FirstOrDefaultAsync(s => s.Id == int.Parse(subjectId));
            if (subject != null)
            {
                subject.Voters.Add(new VoteSubjectInput()
                {
                    Id = 0,
                    VoterId = userId,
                    InputTime = DateTime.UtcNow
                });
                subject.Version = Guid.CreateVersion7();
                await dbContext.SaveChangesAsync();

                result = true;

                LogInformation(logger, $"Successfully gave vote to {subject.Name} ({subjectId})");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while " +
                $"updating vote subject {subjectId}: {ex.Message}. Reverting changes to DB values");
            foreach (var entry in ex.Entries)
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
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while updating vote subject {subjectId}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Task cancelled while updating vote subject {subjectId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while updating vote subject {subjectId}: {ex.Message}",
                ex);
        }

        return result;
    }

    public async Task<bool> DeleteVoteAsync(Vote vote)
    {
        bool result = false;
        if (vote == null)
        {
            return result;
        }

        string id = vote.Id;
        try
        {
            dbContext.Remove(vote);
            var saveResult = await dbContext.SaveChangesAsync();
            result = saveResult > 0;

            if (result)
            {
                LogInformation(logger, $"Successfully deleted vote {vote.Title} ({id})");
            }
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while trying to delete vote {id}: {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while trying to delete vote {id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Task cancelled while trying to delete vote {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while trying to delete vote {id}",
                ex);
        }

        return result;
    }
}