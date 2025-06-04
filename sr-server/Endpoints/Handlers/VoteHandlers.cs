using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Utils.Extensions;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class VoteHandlers
{
    public static RouteGroupBuilder MapVotes(this RouteGroupBuilder routes)
    {
        routes.RequireAuthorization();
        
        routes.MapGet("/", GetMany);
        routes.MapGet("/{id}", Get);
        routes.MapGet("/inputs/user/{user}", GetUserVoteInputs);
        routes.MapPost("/create", Create);
        routes.MapPost("/", Input);
        routes.MapPost("/queue", InputQueue);
        routes.MapDelete("/{id}", Delete);

        return routes;
    }

    public static async Task<IResult> GetMany([AsParameters] VotesQueryDto queryDto,
        [FromServices] IVoteService voteService)
    {
        if (queryDto == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var votes = await voteService.GetVotesAsync(queryDto.Count,
            queryDto.SortBy, queryDto.SortOrder);

        votes ??= [];

        return Results.Ok(ResponseObject.Success(votes.Select(v => v.ToDto())));
    }

    public static async Task<IResult> Get(string? id,
        [FromServices] IVoteService voteService)
    {
        if (id == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var vote = await voteService.GetVoteByIdAsync(id);

        if (vote == null)
        {
            return Results.NotFound(ResponseObject.NotFound());
        }

        return Results.Ok(ResponseObject.Success(vote.ToDto()));
    }

    public static async Task<IResult> GetUserVoteInputs(string? user,
        [FromServices] IVoteService voteService,
        [FromServices] IUserService userService)
    {
        if (user == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingUser = await userService.GetUserByIdAsync(user);
        existingUser ??= await userService.GetUserByEmailAsync(user);

        if (existingUser == null)
        {
            return Results.NotFound(ResponseObject.Create($"User {user} not found"));
        }

        var voteInputs = await voteService.GetVoteInputsByUserIdAsync(existingUser.Id);

        return Results.Ok(ResponseObject.Success(voteInputs.Select(vi => vi.ToDto())));
    }

    public static async Task<IResult> Create(CreateVoteDto? inputDto,
        HttpContext httpContext,
        [FromServices] IVoteService voteService)
    {
        if (inputDto == null
            || !inputDto.IsValid())
        {
            return Results.BadRequest(ResponseObject.BadBody());
        }

        if (httpContext.User == null)
        {
            return Results.LocalRedirect("/accessDenied");
        }

        var creatorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var vote = inputDto.ToVote(creatorId);

        var result = await voteService.AddVoteAsync(vote);
        if (!result)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        try
        {
            var notifier = httpContext.RequestServices.GetRequiredService<IVoteNotificationWriter>();
            await notifier.WriteCreateAsync(vote);
        }
        catch (Exception ex)
        {
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Unexpected error happened while writing created vote notification");
        }

        return Results.Ok(ResponseObject.Success(vote.ToDto()));
    }

    public static async Task<IResult> Input([AsParameters] GiveVoteDto inputVote,
        HttpContext httpContext,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (inputVote == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        if (httpContext.User is not ClaimsPrincipal user)
        {
            return Results.LocalRedirect("/accessDenied");
        }

        var success = false;

        int maxRetry = 3;
        int remainingRetry = maxRetry;

        Vote vote = null!;
        string? email = null;

        var logger = loggerFactory.CreateLogger(nameof(VoteHandlers));
        while (!success && remainingRetry >= 0)
        {
            try
            {
                using var scope = httpContext.RequestServices.CreateScope();
                var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

                vote = (await voteService.GetVoteByIdAsync(inputVote.VoteId))!;

                if (vote == null)
                {
                    return Results.NotFound(ResponseObject.NotFound());
                }

                var inputs = vote.Subjects.SelectMany(s => s.Voters);
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                email = user.FindFirstValue(ClaimTypes.Email);
                if (inputs.Any(i => i.VoterId != null && i.VoterId == userId))
                {
                    return Results.Conflict(ResponseObject.Create($"User {email} already have given vote on vote id {vote.Id}"));
                }

                if (vote.IsClosed()
                    || !vote.CanVote())
                {
                    return Results.Json(ResponseObject.Create("Vote has been closed or exceeded maximum count"),
                        statusCode: StatusCodes.Status403Forbidden);
                }

                if (vote.Subjects.FirstOrDefault(s => s.Id == inputVote.SubjectId) is not VoteSubject subject)
                {
                    return Results.NotFound(ResponseObject.NotFound());
                }

                var result = await voteService.GiveVoteAsync(subject.Id.ToString(), userId);
                if (!result)
                {
                    remainingRetry--;
                }
                else
                {
                    if (remainingRetry < maxRetry)
                    {
                        logger.LogInformation("User {email} succeeded giving vote after retrying for {retryCount} time(s)",
                            email, maxRetry - remainingRetry);
                    }
                    success = true;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                // logger.LogWarning("DB Concurrency happened while {user} giving vote for {vote.Id}", email, vote!.Id);
                remainingRetry--;
            }

            await Task.Delay(Random.Shared.Next(10, 50));
        }

        if (!success)
        {
            logger.LogWarning("User {email} failed giving vote on vote {v.title} ({v.id}) after retrying for {maxRetry}",
                email, vote.Title, vote.Id, maxRetry);
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        try
        {
            var notifier = httpContext.RequestServices.GetRequiredService<IVoteNotificationWriter>();
            await notifier.WriteUpdateAsync(vote);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error happened while writing updated vote notification");
        }

        return Results.Ok(ResponseObject.Success(vote.ToDto()));
    }

    public static async Task<IResult> InputQueue([AsParameters] GiveVoteDto inputVote,
        HttpContext httpContext,
        [FromServices] IVoteQueueWriter voteQueue,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (inputVote == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        if (httpContext.User is not ClaimsPrincipal user)
        {
            return Results.LocalRedirect("/accessDenied");
        }

        var logger = loggerFactory.CreateLogger(nameof(VoteHandlers));
        try
        {
            await voteQueue.WriteAsync(new VoteQueueItem()
            {
                VoteId = inputVote.VoteId,
                SubjectId = inputVote.SubjectId.ToString(),
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier)!
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error happened while user {email} queuing vote {voteId}",
                user.FindFirstValue(ClaimTypes.Email), inputVote.VoteId);
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success($"Vote on {inputVote.VoteId} and subject id {inputVote.SubjectId} was succesfully queued"));
    }

    public static async Task<IResult> Delete(string? id,
        [FromServices] IVoteService voteService)
    {
        if (id == null)
        {
            return Results.BadRequest(ResponseObject.Create("Please provide a valid Id"));
        }

        var vote = await voteService.GetVoteByIdAsync(id);
        if (vote == null)
        {
            return Results.NotFound(ResponseObject.NotFound());
        }

        var result = await voteService.DeleteVoteAsync(vote);
        if (!result)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(null!));

    }
}