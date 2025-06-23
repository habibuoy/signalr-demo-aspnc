using System.Data;
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
using static SignalRDemo.Server.Utils.HttpHelper;
using Microsoft.Extensions.Options;
using SignalRDemo.Server.Configurations;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class VoteHandlers
{
    public static RouteGroupBuilder MapVotes(this RouteGroupBuilder routes)
    {
        routes.RequireAuthorization();

        routes.MapGet("/", GetMany);
        routes.MapGet("/{id}", Get);
        routes.MapGet("/filter-options", GetFilter);
        routes.MapPost("/inputs", Input);
        routes.MapPost("/inputs/queue", InputQueue);

        routes.MapGet("/can-manage", () => Results.Ok(ResponseObject.Create(null!)))
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName);

        routes.MapGet("/inputs/users/{user}", GetUserVoteInputs)
            .RequireAuthorization(VoteInspectorAuthorizationPolicyName);

        routes.MapPost("/", Create)
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName);
        routes.MapPut("/{id}", Update)
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName);
        routes.MapDelete("/{id}", Delete)
            .RequireAuthorization(VoteAdministratorAuthorizationPolicyName);

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
            request.SortBy, request.SortOrder, request.Search);

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

    public static IResult GetFilter([FromServices] IOptions<VoteQueryFilterOptions> filterOptions)
    {
        return Results.Ok(ResponseObject.Success(new
        {
            filterOptions.Value.SortBy,
            filterOptions.Value.SortOrder,
            filterOptions.Value.Search,
        }));
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
        catch (DomainValidationException ex)
        {
            return Results.BadRequest(ResponseObject.ValidationError(ex.ValidationErrors));
        }
        catch (DomainException ex)
        {
            LogError(logger, $"Domain error happened while creating vote entity from user {userEmail} ({userId})",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        var result = await voteService.CreateVoteAsync(vote);
        if (!result.Succeeded)
        {
            return GetHttpResultFromServiceResult(result);
        }

        vote = result.Value;

        try
        {
            var notifier = httpContext.RequestServices.GetRequiredService<IVoteNotificationWriter>();
            await notifier.WriteCreateAsync(vote);
        }
        catch (Exception ex)
        {
            LogError(logger, $"Unexpected error happened while writing created vote notification", ex);
        }

        return Results.Created($"https://{httpContext.Request.Host}/vote/{vote.Id}",
            ResponseObject.Success(vote.ToResponse()));
    }

    public static async Task<IResult> Input([AsParameters] InputVoteRequest request,
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
        VoteSubjectInput voteInput = null!;

        while (!success && remainingRetry >= 0)
        {
            try
            {
                using var scope = httpContext.RequestServices.CreateScope();
                var voteService = scope.ServiceProvider.GetRequiredService<IVoteService>();

                userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                email = httpContext.User.FindFirstValue(ClaimTypes.Email);

                var result = await voteService.GiveVoteAsync(request.SubjectId.ToString(), userId);
                if (!result.Succeeded)
                {
                    return GetHttpResultFromServiceResult(result);
                }

                if (remainingRetry < maxRetry)
                {
                    LogInformation(logger, $"User {email} succeeded giving vote after " +
                        $"retrying for {maxRetry - remainingRetry} time(s)");
                }
                voteInput = result.Value;
                vote = result.Value.VoteSubject!.Vote!;
                success = true;
                break;
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

        return Results.Ok(ResponseObject.Success(voteInput.ToResponse()));
    }

    public static async Task<IResult> InputQueue([AsParameters] InputVoteRequest request,
        HttpContext httpContext,
        [FromServices] IVoteQueueService voteQueueService,
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

        var result = await voteQueueService.QueueVoteAsync(request.SubjectId.ToString(), userId);
        if (!result.Succeeded)
        {
            return GetHttpResultFromServiceResult(result);
        }

        return Results.Ok(ResponseObject.Success(result.Value.ToResponse()));
    }

    public static async Task<IResult> Update(string? id, UpdateVoteRequest? request,
        HttpContext httpContext,
        [FromServices] IVoteService voteService,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (id == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        if (request == null)
        {
            return Results.BadRequest(ResponseObject.BadBody());
        }

        var logger = loggerFactory.CreateLogger(nameof(VoteHandlers));

        var vote = await voteService.GetVoteByIdAsync(id);

        if (vote == null)
        {
            return Results.NotFound(ResponseObject.Create($"Vote with id {id} not found"));
        }

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
            LogError(logger, $"Error happened while validating update vote request from user {email} ({userId}). " +
                $"Field (name: {ex.FieldName}, value: {ex.FieldValue}), reference value: {ex.ReferenceValue}.",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        Vote? updatedVote = null;
        try
        {
            updatedVote = request.ToVote();
        }
        catch (DomainValidationException ex)
        {
            return Results.BadRequest(ResponseObject.ValidationError(ex.ValidationErrors));
        }
        catch (DomainException ex)
        {
            LogError(logger, $"Domain error happened while converting vote request to vote entity " +
                $"from user {email} ({userId})",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        try
        {
            var result = await voteService.UpdateVoteAsync(id, updatedVote);
            if (!result.Succeeded)
            {
                return GetHttpResultFromServiceResult(result);
            }

            updatedVote = result.Value;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogWarning(logger, $"DB concurrency error happened while updating vote entity from user {email} ({userId})",
                ex);
            return Results.Conflict(ResponseObject.Create("Failed to update vote due to the vote was being " +
                "updated by another user. Please try again."));
        }

        return Results.Ok(ResponseObject.Success(updatedVote.ToResponse()));
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
        if (!result.Succeeded)
        {
            return GetHttpResultFromServiceResult(result);
        }

        return Results.Ok(ResponseObject.Success(null!));
    }
}