using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRDemo.Server.Endpoints.Requests;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Utils.Extensions;
using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Configurations.AppConstants;
using static SignalRDemo.Server.Utils.LogHelper;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class VoteHandlers
{
    public static RouteGroupBuilder MapVotes(this RouteGroupBuilder routes)
    {
        routes.RequireAuthorization();

        routes.MapGet("/", GetMany);
        routes.MapGet("/{id}", Get);
        routes.MapPost("/", Input);
        routes.MapPost("/queue", InputQueue);

        routes.MapGet("/can-manage", () => Results.Ok(ResponseObject.Create(null!)))
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName); ;

        routes.MapGet("/inputs/user/{user}", GetUserVoteInputs)
            .RequireAuthorization(VoteInspectorAuthorizationPolicyName);

        routes.MapPost("/create", Create)
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName); ;
        routes.MapDelete("/{id}", Delete)
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName); ;

        return routes;
    }

    public static async Task<IResult> GetMany([AsParameters] VotesQueryRequest request,
        [FromServices] IVoteService voteService)
    {
        if (request == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var votes = await voteService.GetVotesAsync(request.Count,
            request.SortBy, request.SortOrder);

        votes ??= [];

        return Results.Ok(ResponseObject.Success(votes.Select(v => v.ToResponse())));
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

        return Results.Ok(ResponseObject.Success(vote.ToResponse()));
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

        return Results.Ok(ResponseObject.Success(voteInputs.Select(vi => vi.ToResponse())));
    }

    public static async Task<IResult> Create(CreateVoteRequest? request,
        HttpContext httpContext,
        [FromServices] IVoteService voteService,
        [FromServices] IUserService userService,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (request == null)
        {
            return Results.BadRequest(ResponseObject.BadBody());
        }

        var logger = loggerFactory.CreateLogger(nameof(VoteHandlers));
        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userEmail = httpContext.User.FindFirstValue(ClaimTypes.Email);

        try
        {
            var dtoValidation = request.Validate();
            if (!dtoValidation.Succeeded)
            {
                return Results.BadRequest(ResponseObject.ValidationError(dtoValidation.Error));
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            LogError(logger, $"Error happened while validating create vote request from user {userEmail} ({userId}). " +
                $"Field (name: {ex.FieldName}, value: {ex.FieldValue}), reference value: {ex.ReferenceValue}.",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        Vote vote;
        try
        {
            vote = request.ToVote(await userService.GetUserByIdAsync(userId!));
        }
        catch (DomainException ex)
        {
            if (ex.ValidationErrors.Any())
            {
                return Results.BadRequest(ResponseObject.ValidationError(ex.ValidationErrors));
            }
            else
            {
                LogError(logger, $"Domain error happened while creating vote entity from user {userEmail} ({userId})",
                    ex);
                return Results.InternalServerError(ResponseObject.ServerError());
            }
        }

        var result = await voteService.AddVoteAsync(vote);
        if (!result)
        {
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        try
        {
            var notifier = httpContext.RequestServices.GetRequiredService<IVoteNotificationWriter>();
            await notifier.WriteCreateAsync(vote);
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while writing created vote notification", ex);
        }

        return Results.Created($"https://{httpContext.Request.Host}/vote/{vote.Id}", ResponseObject.Success(vote.ToResponse()));
    }

    public static async Task<IResult> Input([AsParameters] GiveVoteRequest request,
        HttpContext httpContext,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (request == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var logger = loggerFactory.CreateLogger(nameof(VoteHandlers));

        string? userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        string? email = httpContext.User.FindFirstValue(ClaimTypes.Email);
        try
        {
            var inputValidation = request.Validate();
            if (!inputValidation.Succeeded)
            {
                return Results.BadRequest(ResponseObject.ValidationError(inputValidation.Error));
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            LogError(logger, $"Error happened while validating input vote request from user {email} ({userId}). " +
                $"Field (name: {ex.FieldName}, value: {ex.FieldValue}), reference value: {ex.ReferenceValue}.",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        var success = false;

        int maxRetry = 3;
        int remainingRetry = maxRetry;

        Vote vote = null!;

        while (!success && remainingRetry >= 0)
        {
            try
            {
                using var scope = httpContext.RequestServices.CreateScope();
                var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

                vote = (await voteService.GetVoteByIdAsync(request.VoteId))!;

                if (vote == null)
                {
                    return Results.NotFound(ResponseObject.NotFound());
                }

                var inputs = vote.Subjects.SelectMany(s => s.Voters);
                userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                email = httpContext.User.FindFirstValue(ClaimTypes.Email);
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

                if (vote.Subjects.FirstOrDefault(s => s.Id == request.SubjectId) is not VoteSubject subject)
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
                        LogInformation(logger, $"User {email} succeeded giving vote after " +
                            $"retrying for {maxRetry - remainingRetry} time(s)");
                    }
                    success = true;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                remainingRetry--;
            }
            catch (Exception ex)
            {
                LogError(logger, $"Unexpected error happened while User {email} " +
                    $"giving vote on vote {vote.Title} ({vote.Id})", ex);
                return Results.InternalServerError(ResponseObject.ServerError());
            }

            await Task.Delay(Random.Shared.Next(10, 50));
        }

        if (!success)
        {
            LogWarning(logger, $"User {email} failed giving vote on vote {vote.Title} ({vote.Id}) " +
                $"after retrying for {maxRetry} times");
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        try
        {
            var notifier = httpContext.RequestServices.GetRequiredService<IVoteNotificationWriter>();
            await notifier.WriteUpdateAsync(vote);
        }
        catch (Exception ex)
        {
            LogError(logger, "Unexpected error happened while writing updated vote notification", ex);
        }

        return Results.Ok(ResponseObject.Success(vote.ToResponse()));
    }

    public static async Task<IResult> InputQueue([AsParameters] GiveVoteRequest request,
        HttpContext httpContext,
        [FromServices] IVoteQueueWriter voteQueue,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (request == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var logger = loggerFactory.CreateLogger(nameof(VoteHandlers));

        string userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        string email = httpContext.User.FindFirstValue(ClaimTypes.Email)!;
        try
        {
            var inputValidation = request.Validate();
            if (!inputValidation.Succeeded)
            {
                return Results.BadRequest(ResponseObject.ValidationError(inputValidation.Error));
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            LogError(logger, $"Error happened while validating input vote queue request from user {email} ({userId}). " +
                $"Field (name: {ex.FieldName}, value: {ex.FieldValue}), reference value: {ex.ReferenceValue}.",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        try
        {
            await voteQueue.WriteAsync(new VoteQueueItem()
            {
                VoteId = request.VoteId,
                SubjectId = request.SubjectId.ToString(),
                UserId = userId
            });
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unknown error happened while user {email} queuing vote {request.VoteId}", ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        return Results.Ok(ResponseObject.Success($"Vote on {request.VoteId} and subject id {request.SubjectId} was succesfully queued"));
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