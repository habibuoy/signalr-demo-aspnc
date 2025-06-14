using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Utils.Extensions;
using SignalRDemo.Server.Utils.Validators;
using static SignalRDemo.Server.Configurations.AppConstants;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class RoleHandlers
{
    public static RouteGroupBuilder MapRoles(this RouteGroupBuilder routes)
    {
        routes.RequireAuthorization(RoleManagerAuthorizationPolicyName);

        routes.MapGet("/{role}", Get);
        routes.MapGet("/{role}/users", GetRoleUsers);
        routes.MapGet("/user/{user}", GetUserRoles);
        routes.MapPost("/create", Create);
        routes.MapPut("/update/{role}", Update);
        routes.MapDelete("/delete/{role}", Delete);
        routes.MapPost("/{role}/assign/{user}", AssignUser);
        routes.MapPost("/{role}/remove/{user}", RemoveUser);

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
        HttpContext httpContext,
        [FromServices] IRoleService roleService,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (inputDto == null)
        {
            return Results.BadRequest(ResponseObject.BadBody());
        }

        var logger = loggerFactory.CreateLogger(nameof(RoleHandlers));

        try
        {
            var validationResult = inputDto.Validate();
            if (!validationResult.Succeeded)
            {
                return Results.BadRequest(ResponseObject.ValidationError(validationResult.Error));
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            var email = httpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
            logger.LogError(ex, "Error happened while validating create role request by {email}. " +
                "Field (name: {fieldName}, value: {fieldValue}), reference value: {refValue}.",
                email, ex.FieldName, ex.FieldValue, ex.ReferenceValue);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        var role = await roleService.GetRoleByNameAsync(inputDto.Name);
        if (role != null)
        {
            return Results.Conflict(ResponseObject.Create($"Role {inputDto.Name} already exists"));
        }

        try
        {
            role = inputDto.ToRole();
        }
        catch (DomainException ex)
        {
            if (ex.ValidationErrors.Any())
            {
                return Results.BadRequest(ResponseObject.ValidationError(ex.ValidationErrors));
            }
            else
            {
                var email = httpContext.User?.FindFirstValue(ClaimTypes.Email);
                logger.LogError(ex, "Domain error happened while creating Role entity by request of user {email}",
                    email);
                return Results.InternalServerError(ResponseObject.ServerError());
            }
        }

        role = await roleService.CreateRoleAsync(role);
        if (role == null)
        {
            return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
        }

        return Results.Ok(ResponseObject.Success(role.ToDto()));
    }

    public static async Task<IResult> Update(string? role, UpdateRoleDto? inputDto,
        HttpContext httpContext,
        [FromServices] IRoleService roleService,
        [FromServices] ILoggerFactory loggerFactory)
    {
        if (role == null || inputDto == null)
        {
            return Results.BadRequest(ResponseObject.BadQuery());
        }

        var logger = loggerFactory.CreateLogger(nameof(RoleHandlers));

        var existingRole = await roleService.GetRoleByNameAsync(role);
        existingRole ??= await roleService.GetRoleByIdAsync(role);

        if (existingRole == null)
        {
            return Results.NotFound(ResponseObject.Create($"Role {role} not found"));
        }

        try
        {
            var validationResult = inputDto.Validate();
            if (!validationResult.Succeeded)
            {
                return Results.BadRequest(ResponseObject.ValidationError(validationResult.Error));
            }
        }
        catch (ModelFieldValidatorException ex)
        {
            var email = httpContext.User?.FindFirst(ClaimTypes.Email)?.Value;
            logger.LogError(ex, "Error happened while validating update role request by {email}. " +
                "Field (name: {fieldName}, value: {fieldValue}), reference value: {refValue}.",
                email, ex.FieldName, ex.FieldValue, ex.ReferenceValue);
            return Results.InternalServerError(ResponseObject.ServerError());
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