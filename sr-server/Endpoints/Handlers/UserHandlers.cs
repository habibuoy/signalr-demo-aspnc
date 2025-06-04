using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Utils.Extensions;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class UserHandlers
{
    public static RouteGroupBuilder MapUsers(this RouteGroupBuilder routes)
    {
        routes.MapGet("/{user}/roles", UserHandlers.GetRoles).RequireAuthorization("RoleManager");

        return routes;
    }

    public static async Task<IResult> GetRoles(string? user,
        [FromServices] IUserService userService,
        [FromServices] IRoleService roleService)
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

        var userRoles = await roleService.GetUserRolesByUserAsync(existingUser);

        return Results.Ok(ResponseObject.Success(userRoles.Select(ur => ur.ToRoleDto())));
    }
}