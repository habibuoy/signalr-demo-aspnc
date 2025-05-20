using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Server;
using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Server.Responses;
using SignalRDemo.Server.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapHub<ChatHub>("/chat");
app.MapHub<VoteHub>("/watchvote");

var votes = new ConcurrentDictionary<string, Vote>();

app.MapGet("/vote/{id}", (string? id) =>
{
    if (id == null)
    {
        return Results.BadRequest(ResponseObject.BadQuery());
    }

    if (!votes.TryGetValue(id, out var vote))
    {
        return Results.NotFound(ResponseObject.NotFound());
    }

    return Results.Ok(ResponseObject.Success(vote.ToDto()));
});

app.MapPost("/vote/create", async (CreateVoteDto? inputDto, HttpContext httpContext) =>
{
    if (inputDto == null
        || !inputDto.IsValid())
    {
        return Results.BadRequest(ResponseObject.BadBody());
    }

    var vote = inputDto.ToVote();
    votes.TryAdd(vote.Id, vote);

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

app.MapPost("/vote", async ([AsParameters] VoteSubjectDto inputVote,
    HttpContext httpContext) =>
{
    if (inputVote == null)
    {
        return Results.BadRequest(ResponseObject.BadQuery());
    }

    if (!votes.TryGetValue(inputVote.VoteId, out var vote))
    {
        return Results.NotFound(ResponseObject.NotFound());
    }

    if (vote.IsClosed()
        || !vote.CanVote())
    {
        return Results.Json(ResponseObject.Create("Vote has been closed or exceeded maximum count"),
            statusCode: StatusCodes.Status403Forbidden);
    }

    if (!vote.SubjectVoteCounts.TryGetValue(inputVote.SubjectId, out var count))
    {
        return Results.NotFound(ResponseObject.NotFound());
    }

    vote.GiveVote(inputVote.SubjectId);
    
    try
    {
        var voteHubContext = httpContext.RequestServices.GetRequiredService<IHubContext<VoteHub, IVoteHubClient>>();
        await voteHubContext.Clients.Group($"vote-{vote.Id}").NotifyVoteUpdated(vote.ToVoteUpdatedProperties());
    }
    catch (InvalidOperationException ex)
    {
        httpContext.RequestServices.GetRequiredService<ILogger<Program>>()
            .LogInformation("Error while hubbing: {msg}", ex.Message);
    }
    
    return Results.Ok(ResponseObject.Success(vote.ToDto()));
});

app.Run();