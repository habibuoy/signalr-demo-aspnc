using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRDemo.Shared;
using SignalRDemo.Shared.Models;
using static SignalRDemo.Shared.HttpHelper;

namespace SignalRDemo.Client;

public class VoteClient
{
    private readonly string endpoint = string.Empty;
    private readonly UserInfo? user;

    private HubConnection? connection;
    private Cookie? cookie;
    private CookieContainer cookieContainer = new();

    public Cookie? Cookie => cookie;

    public HubConnectionState? State => connection?.State;

    public event Action? ConnectionClosed;

    private VoteClient()
    {
        
    }

    public VoteClient(string endpoint, UserInfo user)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentNullException.ThrowIfNull(user);

        this.endpoint = endpoint;
        this.user = user;
    }

    public VoteClient(string endpoint, Cookie cookie)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentNullException.ThrowIfNull(cookie);

        this.endpoint = endpoint;
        this.cookie = cookie;
    }

    public async Task InitializeAsync()
    {
        if (cookie != null)
        {
            try
            {
                cookieContainer.Add(cookie);
            }
            catch (ArgumentNullException ex)
            {
                throw new VoteClientException($"Argument null error happened: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new VoteClientException($"Argument error happened: {ex.Message}");
            }
            catch (CookieException ex)
            {
                throw new VoteClientException($"Provided cookie causing error: {ex.Message}");
            }
        }
        else
        {
            var httpClient = new HttpClient();
            var credentialBody = JsonContent.Create(user);
            var loginResponse = await LoginAsync(httpClient, credentialBody);

            Console.WriteLine($"({nameof(VoteClient)}): Logging in {user!.Email}");
            if (loginResponse.cookie == null
                && loginResponse.statusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"({nameof(VoteClient)}): User {user!.Email} is not registered yet, registering.");
                var registerRespone = await RegisterAsync(httpClient, credentialBody);
                if (!registerRespone.success)
                {
                    throw new VoteClientException($"Failed when registering {user?.Email}: {registerRespone.message}");
                }

                Console.WriteLine($"({nameof(VoteClient)}): Logging in {user!.Email} again");
                loginResponse = await LoginAsync(httpClient, credentialBody);
            }

            if (loginResponse.cookie == null)
            {
                throw new VoteClientException($"Failed when logging in {user?.Email}: {loginResponse.message}");
            }

            Console.WriteLine($"({nameof(VoteClient)}): {user!.Email} Logged in");

            cookie = loginResponse.cookie;
            if (string.IsNullOrEmpty(cookie.Domain))
            {
                string host = new Uri(endpoint).Host;
                cookie.Domain = host;
            }
            cookieContainer.Add(cookie);
        }

        connection = new HubConnectionBuilder()
            .WithUrl(endpoint, configure =>
            {
                configure.Cookies = cookieContainer;
                configure.UseStatefulReconnect = true;
            })
            .Build();

        connection.On<SendMessageProperties>("ReceiveMessage", (props) =>
        {
            Console.WriteLine($"Message received from {props.Sender}: {props.Message}");
        });

        connection.On<VoteCreatedProperties>("NotifyVoteCreated", (props) =>
        {
            Console.WriteLine($"New vote has been created, Id: {props.Id}, Title: {props.Title}, Subject count: {props.Subjects.Count}");
        });

        connection.On<VoteUpdatedProperties>("NotifyVoteUpdated", (props) =>
        {
            Console.WriteLine($"Vote Id: {props.Id}, Title: {props.Title} has been updated, total count: {props.TotalCount}");
        });

        connection.Closed += OnConnectionClosed;
        Console.WriteLine($"({nameof(VoteClient)}): Connecting to the server");

        try
        {
            await connection.StartAsync();
            Console.WriteLine($"({nameof(VoteClient)}): Connected to the server");
        }
        catch (HttpRequestException httpExc)
        {
            throw new VoteClientException($"Http Error happened while connecting to the server: {httpExc.Message}");
        }
        catch (HubException ex)
        {
            throw new VoteClientException($"Hub Error happened while connecting to the server: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new VoteClientException($"Unknown Error happened while connecting to the server", ex);
        }
    }

    public async Task CloseAsync()
    {
        Console.WriteLine($"({nameof(VoteClient)}): Stopping connection");
        try
        {
            await connection!.StopAsync();
            Console.WriteLine($"({nameof(VoteClient)}): Connection stopped");
        }
        catch (HubException ex)
        {
            throw new VoteClientException($"Error happened while stopping connection to the server: {ex.Message}.");
        }
        catch (NullReferenceException)
        {
            throw new VoteClientException($"Error happened while stopping connection. Connection is not valid.");
        }
        catch (Exception ex)
        {
            throw new VoteClientException($"Unknown Error happened while stopping connection to the server: {ex.Message}.");
        }
    }

    public async Task SendMessageAsync(string message)
    {
        var command = string.Empty;
        try
        {
            if (message.StartsWith("subsvote", StringComparison.CurrentCultureIgnoreCase))
            {
                command = "SubscribeVote";
            }
            else if (message.StartsWith("unsubsvote", StringComparison.CurrentCultureIgnoreCase))
            {
                command = "UnsubscribeVote";
            }

            if (!string.IsNullOrEmpty(command))
            {
                var voteId = message.Split(" ")[1];
                var result = await connection!.InvokeAsync<InvocationResult>(command, voteId);
                if (result.IsSuccess)
                {
                    Console.WriteLine($"({nameof(VoteClient)}): Succesfully invoking command {command} of vote {voteId}");
                }
                else
                {
                    Console.WriteLine($"({nameof(VoteClient)}): Failed to invoke command {command} of vote {voteId}: {result.Message}");
                }
                return;
            }

            await connection!.SendAsync("Send", new { Sender = user!.FirstName, Message = message });
        }
        catch (InvalidOperationException)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new VoteClientException("Error happened while sending message. Connection has been closed.");
            }
            else
            {
                throw new VoteClientException($"Error happened while invoking command {command}. Connection has been closed.");
            }
        }
        catch (NullReferenceException ex)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new VoteClientException($"Error happened while sending message. Connection is not valid.", ex);
            }
            else
            {
                throw new VoteClientException($"Error happened while invoking command {command}. Connection is not valid.", ex);
            }
        }
        catch (HubException ex)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new VoteClientException($"Hub Error happened while sending message.", ex);
            }
            else
            {
                throw new VoteClientException($"Hub Error happened while invoking command {command}.", ex);
            }
        }
        catch (Exception ex)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new VoteClientException($"Unknown Error happened while sending message.", ex);
            }
            else
            {
                throw new VoteClientException($"Unknown Error happened while invoking command {command}.", ex);
            }
        }
    }

    private Task OnConnectionClosed(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"({nameof(VoteClient)}): Connection was closed due to connection error: {ex}");
        }
        else
        {
            Console.WriteLine($"({nameof(VoteClient)}): Connection was closed by either server or the client");
        }

        if (connection != null)
        {
            connection.Closed -= OnConnectionClosed;
        }

        ConnectionClosed?.Invoke();
        return Task.CompletedTask;
    }
}

public class VoteClientException : Exception
{
    public VoteClientException(string? message) 
        : base(message) { }

    public VoteClientException(string? message, Exception? innerException) 
        : base(message, innerException) { }
}