using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Utils.Extensions;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class RoleHandlers
{
    public static RouteGroupBuilder MapRoles(this RouteGroupBuilder routes)
    {
        routes.MapGet("/{role}", RoleHandlers.Get).RequireAuthorization("RoleManager");
        routes.MapGet("/{role}/users", RoleHandlers.GetRoleUsers).RequireAuthorization("RoleManager");
        routes.MapGet("/user/{user}", RoleHandlers.GetUserRoles).RequireAuthorization("RoleManager");
        routes.MapPost("/create", RoleHandlers.Create).RequireAuthorization("RoleManager");
        routes.MapPut("/update/{role}", RoleHandlers.Update).RequireAuthorization("RoleManager");
        routes.MapDelete("/delete/{role}", RoleHandlers.Delete).RequireAuthorization("RoleManager");
        routes.MapPost("/{role}/assign/{user}", RoleHandlers.AssignUser).RequireAuthorization("RoleManager");
        routes.MapPost("/{role}/remove/{user}", RoleHandlers.RemoveUser).RequireAuthorization("RoleManager");

        return routes;
    }

    public static async Task<IResult> Get(string? role,
        [FromServices] IRoleService roleService)
    {
        if (role == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        return Results.Ok(ResponseObject.Success(existingRole.ToDto()));
    }

    public static async Task<IResult> GetRoleUsers(string? role,
        [FromServices] IRoleService roleService)
    {
        if (role == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        var userRoles = await roleService.GetUserRolesByRoleAsync(existingRole);

        return Results.Ok(ResponseObject.Success(userRoles.Select(ur => ur.ToUserDto())));
    }

    public static async Task<IResult> GetUserRoles(string? user,
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

    public static async Task<IResult> Create(CreateRoleDto? inputDto,
        [FromServices] IRoleService roleService)
    {
        if (inputDto == null)
        {
            return Results.BadRequest(ResponseObject.BadBody());
        }

        var role = await roleService.GetRoleByNameAsync(inputDto.Name);
        if (role != null)
        {
            return Results.Conflict(ResponseObject.Create($"Role {inputDto.Name} already exists"));
        }

        role = await roleService.CreateRoleAsync(inputDto.Name, inputDto.Description);
        if (role == null)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(role.ToDto()));
    }

    public static async Task<IResult> Update(string? role, UpdateRoleDto? inputDto,
        [FromServices] IRoleService roleService)
    {
        if (role == null || inputDto == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        var targetRole = await roleService.GetRoleByNameAsync(inputDto.Name);
        if (targetRole != null)
        {
            return Results.Conflict(ResponseObject.Create($"Role {inputDto.Name} already exists"));
        }

        var updatedRole = inputDto.ToRole(existingRole.Id);

        var result = await roleService.UpdateRoleAsync(updatedRole);
        if (!result)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(existingRole.ToDto()));
    }

    public static async Task<IResult> Delete(string? role,
        [FromServices] IRoleService roleService)
    {
        if (role == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        var result = await roleService.DeleteRoleAsync(existingRole);
        if (!result)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(null!));
    }

    public static async Task<IResult> AssignUser(string? user, string? role,
        [FromServices] IUserService userService,
        [FromServices] IRoleService roleService)
    {
        if (user == null || role == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingUser = await userService.GetUserByIdAsync(user);
        existingUser ??= await userService.GetUserByEmailAsync(user);

        if (existingUser == null)
        {
            return Results.NotFound(ResponseObject.Create($"User with ID {user} not found"));
        }

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        var userRole = await roleService.GetUserRoleAsync(existingUser, existingRole);
        if (userRole != null)
        {
            return Results.Conflict(ResponseObject.Create($"User {existingUser.Email} already has role {existingRole.Name}"));
        }

        userRole = await roleService.AssignUserToRoleAsync(existingUser, existingRole);
        if (userRole == null)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(userRole.ToDto()));
    }

    public static async Task<IResult> RemoveUser(string? user, string? role,
        [FromServices] IUserService userService,
        [FromServices] IRoleService roleService)
    {
        if (user == null || role == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var existingUser = await userService.GetUserByIdAsync(user);
        existingUser ??= await userService.GetUserByEmailAsync(user);

        if (existingUser == null)
        {
            return Results.NotFound(ResponseObject.Create($"User with ID {user} not found"));
        }

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        var userRole = await roleService.GetUserRoleAsync(existingUser, existingRole);
        if (userRole == null)
        {
            return Results.BadRequest(ResponseObject.Create($"User {existingUser.Email} does not have role {existingRole.Name}"));
        }

        var result = await roleService.RemoveUserFromRoleAsync(existingUser, existingRole);
        if (!result)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(null!));
    }
}