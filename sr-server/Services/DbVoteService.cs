using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SignalRDemo.Server.Configurations;
using SignalRDemo.Server.Datas;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using static SignalRDemo.Server.Utils.LogHelper;

namespace SignalRDemo.Server.Services;

public class DbVoteService : IVoteService, IVoteQueueService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IVoteQueueWriter queueWriter;
    private readonly ILogger<DbVoteService> logger;
    private readonly IOptions<VoteQueryFilterOptions> filterOptions;

    public DbVoteService(ApplicationDbContext dbContext,
        IVoteQueueWriter queueWriter,
        IOptions<VoteQueryFilterOptions> filterOptions,
        ILogger<DbVoteService> logger)
    {
        this.dbContext = dbContext;
        this.queueWriter = queueWriter;
        this.filterOptions = filterOptions;
        this.logger = logger;
    }

    public async Task<ServiceResult<Vote>> CreateVoteAsync(Vote vote)
    {
        try
        {
            // clean the ids, let the EF Core generate them
            foreach (var subject in vote.Subjects)
            {
                subject.Id = 0;
            }

            dbContext.Add(vote);
            var success = await dbContext.SaveChangesAsync() > 0;

            if (!success)
            {
                LogWarning(logger, $"Failed to create vote {vote.Title}");
                return ServiceResult<Vote>.SimpleSystemError($"Failed to create vote {vote.Title}");
            }

            LogInformation(logger, $"Successfully created vote {vote.Title} ({vote.Id})");
            return ServiceResult<Vote>.Succeed(vote);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while trying to create vote {vote.Id}: {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            LogWarning(logger, $"DB error happened while trying to create vote {vote.Id}: {ex.Message}");
        }
        catch (OperationCanceledException ex)
        {
            LogWarning(logger, $"Task cancelled while trying to create vote {vote.Id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while trying to create vote {vote.Id}",
                ex);
        }

        return ServiceResult<Vote>.SimpleSystemError($"Failed to create vote {vote.Title}");
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
        string? search = null)
    {
        var votes = dbContext.Votes
            .Include(v => v.Subjects)
            .ThenInclude(vs => vs.Voters)
            .AsSplitQuery();

        if (search != null)
        {
            votes = votes
                .Include(v => v.User)
                .Where(v => v.Title.Contains(search) || v.User!.Email.Contains(search));
        }

        if (sortBy == null
            || !filterOptions.Value.SorterExpressions.TryGetValue(sortBy!, out var expression))
        {
            expression = filterOptions.Value.DefaultSorterExpression;
        }

        sortOrder ??= filterOptions.Value.DefaultSortOrderOption.Value;
        if (sortOrder == QuerySortOrderOptions.Ascending.Value)
        {
            votes = votes.OrderBy(expression);
        }
        else
        {
            votes = votes.OrderByDescending(expression);
        }

        votes = votes.Take(count!.Value);
        var result = await votes.ToListAsync();

        return result;
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

    public async Task<ServiceResult<Vote>> UpdateVoteAsync(string voteId, Vote vote)
    {
        string id = voteId;

        try
        {
            var existing = await dbContext.Votes.FindAsync(id);
            if (existing != null)
            {
                try
                {
                    existing.UpdateFrom(vote);
                }
                catch (DomainValidationException ex)
                {
                    LogWarning(logger, $"Trying to update vote {id} with invalid fields");
                    return ServiceResult<Vote>.ValidationError(ex.ValidationErrors);
                }
                catch (DomainException ex)
                {
                    LogError(logger, $"Domain error happened while updating vote {existing.Title} ({id})",
                        ex);
                    return ServiceResult<Vote>.SimpleSystemError(
                        $"Domain error happened while updating vote {existing.Title} ({id}): {ex.Message}");
                }

                await dbContext.SaveChangesAsync();

                LogInformation(logger, $"Successfully updated vote {existing.Title} ({id})");

                return ServiceResult<Vote>.Succeed(existing);
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

        return ServiceResult<Vote>.SimpleSystemError($"Failed to update vote {id}");
    }

    public async Task<ServiceResult<VoteSubjectInput>> GiveVoteAsync(string subjectId, string? userId)
    {
        try
        {
            var result = await ValidateVoteInput(subjectId, userId);

            if (!result.Succeeded)
            {
                return ServiceResult<VoteSubjectInput>.Fail(result.ErrorCode, result.Error);
            }

            var subject = result.Value.Item1;
            var voteInput = result.Value.Item2;

            subject.Voters.Add(voteInput);
            subject.Version = Guid.CreateVersion7();
            await dbContext.SaveChangesAsync();

            LogInformation(logger, $"Successfully gave vote to {subject.Name} ({subjectId})");

            return ServiceResult<VoteSubjectInput>.Succeed(voteInput);
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

        return ServiceResult<VoteSubjectInput>.SimpleSystemError($"Failed to give vote to subject {subjectId}");
    }

    public async Task<ServiceResult<Vote>> DeleteVoteAsync(Vote vote)
    {
        if (vote == null)
        {
            LogWarning(logger, $"Trying to delete a null vote");
            return ServiceResult<Vote>.SimpleSystemError("Cannot delete a null vote");
        }

        string id = vote.Id;
        try
        {
            dbContext.Remove(vote);
            var success = await dbContext.SaveChangesAsync() > 0;

            if (!success)
            {
                LogWarning(logger, $"Failed to delete vote {vote.Title}");
                return ServiceResult<Vote>.SimpleSystemError($"Failed to delete vote {vote.Title} ({id})");
            }

            LogInformation(logger, $"Successfully deleted vote {vote.Title} ({id})");
            return ServiceResult<Vote>.Succeed(vote);
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

        return ServiceResult<Vote>.SimpleSystemError($"Failed to delete vote {vote.Title} ({vote.Id})");
    }

    public async Task<ServiceResult<VoteQueueInput>> QueueVoteAsync(string subjectId, string? userId)
    {
        var result = await ValidateVoteInput(subjectId, userId);

        if (!result.Succeeded)
        {
            return ServiceResult<VoteQueueInput>.Fail(result.ErrorCode, result.Error);
        }

        var subject = result.Value.Item1;
        var voteInput = result.Value.Item2;

        try
        {
            await queueWriter.WriteAsync(new VoteQueueItem()
            {
                VoteId = subject.Vote!.Id,
                SubjectId = subjectId,
                UserId = voteInput.VoterId
            });
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while writing vote queue on subject id {subjectId} " +
                $"from userId {voteInput.VoterId}", ex);
            return ServiceResult<VoteQueueInput>.SimpleSystemError($"Failed to queue vote input on {subjectId}");
        }

        var queueInput = VoteQueueInput.Create(subject.Vote!.Id, subjectId, voteInput.VoterId);
        return ServiceResult<VoteQueueInput>.Succeed(queueInput);
    }

    private async Task<ServiceResult<(VoteSubject, VoteSubjectInput)>>
        ValidateVoteInput(string subjectId, string? userId)
    {
        if (!int.TryParse(subjectId, out var subjId))
        {
            LogWarning(logger, $"Failed to give vote to a subject with invalid id {subjectId}");
            return ServiceResult<(VoteSubject, VoteSubjectInput)>.SimpleFail(GenericServiceErrorCode.InvalidObject,
                $"Subject id {subjectId} is invalid");
        }

        var vote = await dbContext.Votes
            .Include(v => v.Subjects)
            .ThenInclude(s => s.Voters)
            .AsSplitQuery()
            .FirstOrDefaultAsync(v => v.Subjects.Any(s => s.Id == subjId));

        if (vote == null)
        {
            LogWarning(logger, $"Vote related to subject with id {subjectId} not found while trying to give vote");
            return ServiceResult<(VoteSubject, VoteSubjectInput)>.SimpleFail(GenericServiceErrorCode.NotFound,
                $"Vote related with subject id {subjectId} not found");
        }

        var subject = vote.Subjects.FirstOrDefault(s => s.Id == subjId);

        if (subject == null)
        {
            LogWarning(logger, $"Subject with id {subjectId} not found while trying to give vote");
            return ServiceResult<(VoteSubject, VoteSubjectInput)>.SimpleFail(GenericServiceErrorCode.NotFound,
                $"Subject id {subjectId} not found");
        }

        if (vote.IsClosed()
            || !vote.CanVote())
        {
            LogWarning(logger, $"Failed to give vote to subject {subject.Name} ({subjectId}). " +
                $"{vote.Title} ({vote.Id}) related to it is closed or has exceeded maximum count");
            return ServiceResult<(VoteSubject, VoteSubjectInput)>.SimpleFail(GenericServiceErrorCode.Conflicted,
                $"Vote is closed or has exceeded maximum count");
        }

        if (userId != null
            && vote.Subjects.SelectMany(s => s.Voters)
                .FirstOrDefault(i => i.VoterId == userId) is not null)
        {
            LogWarning(logger, $"User {userId} trying to give vote on Vote {subject.VoteId} while it has given " +
                $"its vote on Vote {subject.VoteId}");
            return ServiceResult<(VoteSubject, VoteSubjectInput)>.SimpleFail(GenericServiceErrorCode.Conflicted,
                $"User {userId} has already given vote on Vote {subject.VoteId}");
        }

        var voteInput = VoteSubjectInput.Create(subject!.Id, userId);

        return ServiceResult<(VoteSubject, VoteSubjectInput)>.Succeed((subject, voteInput));
    }
}