using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Server.Endpoints.Requests;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Utils.Extensions;
using SignalRDemo.Server.Validations;
using static SignalRDemo.Server.Utils.LogHelper;

namespace SignalRDemo.Server.Endpoints.Handlers;

public static class RootHandlers
{
    public static RouteGroupBuilder MapRoots(this RouteGroupBuilder routes)
    {
        routes.MapPost("/register", Register);
        routes.MapPost("/login", Login);
        routes.MapGet("/logout", Logout).RequireAuthorization();
        routes.MapGet("/accessDenied", AccessDenied);
        return routes;
    }

    public static async Task<IResult> Register(CreateUserRequest request,
        [FromServices] IUserService userService,
        [FromServices] ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(nameof(RootHandlers));

        try
        {
            var validation = request.Validate();
            if (!validation.Succeeded) return Results.BadRequest(ResponseObject.ValidationError(validation.Error));
        }
        catch (ModelFieldValidatorException ex)
        {
            LogError(logger, "Error happened while validating register request. " +
                $"Field (name: {ex.FieldName}, value: {ex.FieldValue}), reference value: {ex.ReferenceValue}.",
                ex);
            return Results.InternalServerError(ResponseObject.ServerError());
        }

        var email = request.Email;
        var user = await userService.GetUserByEmailAsync(email);
        if (user != null)
        {
            return Results.Conflict(ResponseObject.Create($"Email {email} already registered"));
        }

        try
        {
            user = request.ToUser();
        }
        catch (DomainException ex)
        {
            if (ex.ValidationErrors.Any())
            {
                return Results.BadRequest(ResponseObject.ValidationError(ex.ValidationErrors));
            }
            else
            {
                LogError(logger, $"Domain error happened while creating User entity of {email}",
                    ex);
                return Results.InternalServerError(ResponseObject.ServerError());
            }
        }

        user = await userService.CreateUserAsync(user);
        if (user == null)
        {
            return Results.InternalServerError(ResponseObject.Create(
                $"Error on our side while registering {email}"));
        }

        return Results.Ok(ResponseObject.Success(user.ToResponse()));
    }

    public static async Task<IResult> Login(LoginUserRequest request,
        HttpContext httpContext,
        [FromServices] IUserService userService,
        [FromServices] IRoleService roleService)
    {
        var email = request.Email;
        var user = await userService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return Results.NotFound(ResponseObject.Create($"Email {email} is not registered"));
        }

        var valid = await userService.AuthenticateAsync(user, request.Password);
        if (!valid)
        {
            return Results.BadRequest(ResponseObject.Create($"Invalid email or password"));
        }

        var claims = new List<Claim>()
        {
            new (ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new (ClaimTypes.Email, email),
            new (ClaimTypes.NameIdentifier, user.Id)
        };

        var roles = await roleService.GetUserRolesByUserAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r.Role!.Name)));

        var claimsIdentity = new ClaimsIdentity(claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authenticationProperties = new AuthenticationProperties
        {
            IssuedUtc = DateTime.UtcNow,
        };

        await httpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity), authenticationProperties);

        return Results.Ok(ResponseObject.Success(user.ToResponse()));
    }

    public static async Task<IResult> Logout(HttpContext httpContext,
        [FromServices] IUserService userService)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok(ResponseObject.Success(null!));
    }

    public static IResult AccessDenied() =>
        Results.Json(ResponseObject.NotAuthorized(), statusCode: StatusCodes.Status401Unauthorized);

}