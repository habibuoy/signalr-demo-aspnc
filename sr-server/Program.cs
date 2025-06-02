using SignalRDemo.Server;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Services;
using SignalRDemo.Server.Datas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using SignalRDemo.Server.SignalRHubs;
using SignalRDemo.Server.Utils.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("MainDb");

builder.Services.AddSqlite<ApplicationDbContext>(connectionString,
    optionsAction: options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.ConfigureWarnings(w =>
            {
                w.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
            });
        }
    }
);

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.AccessDeniedPath = "/accessDenied";
        options.LoginPath = "/accessDenied";
        options.SlidingExpiration = true;
        options.Cookie.Name = "auth";
        // TODO: Change below properties back to secure ones if the web client is moved to same project
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.HttpOnly = false;
    }
);
builder.Services.AddAuthorization();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IVoteService, DbVoteService>();
builder.Services.AddSingleton<IHubConnectionManager, HubConnectionManager>();
builder.Services.AddSingleton<IHubConnectionReader, HubConnectionManager>();
var voteNotification = new VoteNotification();
builder.Services.AddSingleton<IVoteNotificationReader>(voteNotification);
builder.Services.AddSingleton<IVoteNotificationWriter>(voteNotification);
builder.Services.AddHostedService<VoteBroadcasterBackgroundService>();

var app = builder.Build();

app.Lifetime.ApplicationStopping.Register(async () =>
{
    await voteNotification.CloseAsync();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chat");
app.MapHub<VoteHub>("/watchvote", configure =>
    {
        configure.CloseOnAuthenticationExpiration = true;
    })
    .RequireAuthorization();

app.MapPost("/register", async (CreateUserDto userDto,
    [FromServices] IUserService userService) =>
{
    var email = userDto.Email;
    var user = await userService.FindUserByEmailAsync(email);
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
});

app.MapPost("/login", async (LoginUserDto userDto,
    HttpContext httpContext,
    [FromServices] IUserService userService) =>
{
    var email = userDto.Email;
    var user = await userService.FindUserByEmailAsync(email);
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

    var claimsIdentity = new ClaimsIdentity(claims,
        CookieAuthenticationDefaults.AuthenticationScheme);

    var authenticationProperties = new AuthenticationProperties
    {
        IssuedUtc = DateTime.UtcNow,
        // ExpiresUtc = DateTime.UtcNow.AddSeconds(30)
    };

    await httpContext.SignInAsync(new ClaimsPrincipal(claimsIdentity), authenticationProperties);

    return Results.Ok(user.ToDto());
});

app.MapGet("/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok(ResponseObject.Success(null!));
}).RequireAuthorization();

app.MapGet("/accessDenied", () =>
{
    return Results.Json(ResponseObject.NotAuthorized(),
        statusCode: StatusCodes.Status401Unauthorized);
});

app.MapGet("/votes", async ([AsParameters] VotesQueryDto queryDto,
    [FromServices] IVoteService voteService) =>
{
    if (queryDto == null)
    {
        return Results.BadRequest(ResponseObject.BadQuery());
    }

    var votes = await voteService.GetVotesAsync(queryDto.Count,
        queryDto.SortBy, queryDto.SortOrder);

    votes ??= [];

    return Results.Ok(ResponseObject.Success(votes.Select(v => v.ToDto())));
}).RequireAuthorization();

app.MapGet("/vote/{id}", async (string? id,
    [FromServices] IVoteService voteService) =>
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
}).RequireAuthorization();

app.MapPost("/vote/create", async (CreateVoteDto? inputDto,
    HttpContext httpContext, [FromServices] IVoteService voteService) =>
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
}).RequireAuthorization();

app.MapPost("/vote", async ([AsParameters] GiveVoteDto inputVote,
    HttpContext httpContext,
    [FromServices] ILogger<Program> logger) =>
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
}).RequireAuthorization();

app.MapDelete("/vote/{id}", async (string? id,
    [FromServices] IVoteService voteService) =>
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

}).RequireAuthorization();

app.MapGet("/vote/inputs", async (HttpContext httpContext,
    [FromServices] IVoteService voteService) =>
{
    if (httpContext.User is not ClaimsPrincipal user)
    {
        return Results.LocalRedirect("/accessDenied");
    }

    var votes = await voteService.GetVoteInputsByUserIdAsync(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
    return Results.Ok(ResponseObject.Success(votes.Select(v => v.ToDto())));

}).RequireAuthorization();

app.Run();