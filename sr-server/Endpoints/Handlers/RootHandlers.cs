using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Utils.Extensions;

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

    public static async Task<IResult> Register(CreateUserDto userDto,
        [FromServices] IUserService userService)
    {
        var email = userDto.Email;
        var user = await userService.GetUserByEmailAsync(email);
        if (user != null)
        {
            return Results.Conflict(ResponseObject.Create($"Email {email} already registered"));
        }

        user = await userService.CreateUserAsync(userDto.Email,
        userDto.Password, userDto.FirstName, userDto.LastName);
        if (user == null)
        {
            return Results.InternalServerError(ResponseObject.Create(
                $"Error on our side while registering {email}"));
        }

        return Results.Ok(user.ToDto());
    }

    public static async Task<IResult> Login(LoginUserDto userDto,
        HttpContext httpContext,
        [FromServices] IUserService userService,
        [FromServices] IRoleService roleService)
    {
        var email = userDto.Email;
        var user = await userService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return Results.NotFound(ResponseObject.Create($"Email {email} is not registered"));
        }

        var valid = await userService.AuthenticateAsync(user, userDto.Password);
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
            IssuedUtc = DateTime.UtcNow
        };

        await httpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity), authenticationProperties);

        return Results.Ok(user.ToDto());
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