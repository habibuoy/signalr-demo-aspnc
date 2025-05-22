using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Interfaces;
using SignalRDemo.Server.Services;
using SignalRDemo.Server.Datas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

var connectionString = builder.Configuration.GetConnectionString("MainDb");

builder.Services.AddSqlite<ApplicationDbContext>(connectionString,
    optionsAction: options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
    });
builder.Services.AddTransient<IVoteService, DbVoteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chat");
app.MapHub<VoteHub>("/watchvote");

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
});

app.MapPost("/vote/create", async (CreateVoteDto? inputDto,
    HttpContext httpContext, [FromServices] IVoteService voteService) =>
{
    if (inputDto == null
        || !inputDto.IsValid())
    {
        return Results.BadRequest(ResponseObject.BadBody());
    }

    var vote = inputDto.ToVote();

    var result = await voteService.AddVoteAsync(vote);
    if (!result)
    {
        return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
    }

    try
    {
        var voteHubContext = httpContext.RequestServices.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
        await voteHubContext.Clients.All.NotifyVoteCreated(vote.ToVoteCreatedProperties());
    }
    catch (InvalidOperationException ex)
    {
        httpContext.RequestServices.GetRequiredService<ILogger<Program>>()
            .LogInformation("Error while hubbing: {msg}", ex.Message);
    }

    return Results.Ok(ResponseObject.Success(vote.ToDto()));
});

app.MapPost("/vote", async ([AsParameters] GiveVoteDto inputVote,
    HttpContext httpContext, [FromServices] IVoteService voteService) =>
{
    if (inputVote == null)
    {
        return Results.BadRequest(ResponseObject.BadQuery());
    }

    var vote = await voteService.GetVoteByIdAsync(inputVote.VoteId);

    if (vote == null)
    {
        return Results.NotFound(ResponseObject.NotFound());
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

    var success = false;

    // Can put a retry logic here in case of concurrency exception
    int remainingRetry = 3;

    while (!success && remainingRetry > 0)
    {
        try
        {
            var result = await voteService.GiveVoteAsync(inputVote.SubjectId.ToString(), inputVote.UserId);
            if (!result)
            {
                remainingRetry--;
            }
            else
            {
                success = true;
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            remainingRetry--;
        }

        await Task.Delay(Random.Shared.Next(10, 50));
    }

    if (!success)
    {
        return Results.InternalServerError(ResponseObject.Create("There was an error on our side"));
    }

    try
    {
        var voteHubContext = httpContext.RequestServices.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
        // await voteHubContext.Clients.Group($"vote-{vote.Id}").NotifyVoteUpdated(vote.ToVoteUpdatedProperties());
        await voteHubContext.Clients.All.NotifyVoteUpdated(vote.ToVoteUpdatedProperties());
    }
    catch (InvalidOperationException ex)
    {
        httpContext.RequestServices.GetRequiredService<ILogger<Program>>()
            .LogInformation("Error while hubbing: {msg}", ex.Message);
    }
    
    return Results.Ok(ResponseObject.Success(vote.ToDto()));
});

app.Run();