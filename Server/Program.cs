using SimpleVote.Server.Configurations;
using SimpleVote.Server.Endpoints.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataServices(builder.Configuration, builder.Environment);
builder.Services.AddGeneralServices();
builder.Services.AddDomainServices();
builder.Services.AddOptionServices();
builder.Services.AddSecurityServices();

var app = builder.Build();

app.Lifetime.ConfigureLifetime(app.Services);
app.UseMiddlewares();

app.MapAppEndpoints();

app.MapSignalRs();
app.MapGroup("/").MapRoots();
app.MapGroup("/users").MapUsers();
app.MapGroup("/roles").MapRoles();
app.MapGroup("/votes").MapVotes();

await app.ConfigureUsers();

app.Run();