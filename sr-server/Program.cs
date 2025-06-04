using SignalRDemo.Server.Configurations;
using SignalRDemo.Server.Endpoints.Handlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataServices(builder.Configuration, builder.Environment);
builder.Services.AddGeneralServices();
builder.Services.AddDomainServices();
builder.Services.AddSecurityServices();

var app = builder.Build();

app.Lifetime.ConfigureLifetime(app.Services);
app.UseMiddlewares();

app.MapSignalRs();
app.MapGroup("/").MapRoots();
app.MapGroup("/user").MapUsers();
app.MapGroup("/role").MapRoles();
app.MapGroup("/vote").MapVotes();

app.Run();