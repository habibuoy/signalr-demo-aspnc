using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Utils.Extensions;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class UserHandlers
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder routes)
    {
        routes.MapGet("/roles", UserHandlers.GetRoles).RequireAuthorization("RoleManager");
        routes.MapGet("/vote-inputs", UserHandlers.GetVoteInputs).RequireAuthorization();

        return routes;
    }

    public static async Task<IResult> GetRoles(HttpContext httpContext,
        [FromServices] IRoleService roleService,
        [FromServices] IUserService userService)
    {
        var user = httpContext.User;
        if (user == null)
        {
            return Results.BadRequest(ResponseObject.NotAuthorized());
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingUser = await userService.GetUserByIdAsync(userId!);
        if (existingUser == null)
        {
            return Results.NotFound(ResponseObject.Create($"User {userId} not found"));
        }

        var userRoles = await roleService.GetUserRolesByUserAsync(existingUser);

        return Results.Ok(ResponseObject.Success(userRoles.Select(ur => ur.ToRoleDto())));
    }

    public static async Task<IResult> GetVoteInputs( HttpContext httpContext,
        [FromServices] IVoteService voteService)
    {
        var existingUser = httpContext.User;

        if (existingUser == null)
        {
            return Results.NotFound(ResponseObject.NotAuthorized());
        }

        var votes = await voteService.GetVoteInputsByUserIdAsync(existingUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Results.Ok(ResponseObject.Success(votes.Select(v => v.ToDto())));
    }
}