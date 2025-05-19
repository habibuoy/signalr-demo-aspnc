using System.Collections.Concurrent;
using SignalRDemo.Server;
using SignalRDemo.Server.Model;
using SignalRDemo.Server.Response;

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

app.MapPost("/vote/create", (CreateVoteDto? inputDto) =>
{
    if (inputDto == null
        || !inputDto.IsValid())
    {
        return Results.BadRequest(ResponseObject.BadBody());
    }

    var vote = inputDto.ToVote();
    votes.TryAdd(vote.Id, vote);

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
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        await httpContext.Response.WriteAsJsonAsync(
            ResponseObject.Create("Vote has been closed or exceeded maximum count"));
    }

    if (!vote.SubjectVoteCounts.TryGetValue(inputVote.SubjectId, out var count))
    {
        return Results.NotFound(ResponseObject.NotFound());
    }

    vote.GiveVote(inputVote.SubjectId);
    return Results.Ok(ResponseObject.Success(vote.ToDto()));
});

app.Run();