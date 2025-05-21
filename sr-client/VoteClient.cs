using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRDemo.Shared;

namespace SignalRDemo.Client;

public class VoteClient
{
    private readonly string endpoint;
    private readonly string user;

    private HubConnection connection;

    public HubConnectionState State => connection.State;

    public event Action ConnectionClosed;

    public VoteClient(string endpoint, string user)
    {
        ArgumentException.ThrowIfNullOrEmpty(endpoint);
        ArgumentException.ThrowIfNullOrEmpty(user);

        this.endpoint = endpoint;
        this.user = user;
    }

    public async Task InitializeAsync()
    {
        connection = new HubConnectionBuilder()
            .WithUrl(endpoint)
            .Build();

        connection.On<SendMessageProperties>("ReceiveMessage", (props) =>
        {
            Console.WriteLine($"Message received from {props.Sender}: {props.Message}");
        });

        connection.On<VoteCreatedProperties>("NotifyVoteCreated", (props) =>
        {
            Console.WriteLine($"New vote has been created, Id: {props.Id}, Title: {props.Title}, Subject count: {props.SubjectCount}");
        });

        connection.On<VoteUpdatedProperties>("NotifyVoteUpdated", (props) =>
        {
            Console.WriteLine($"Vote Id: {props.Id}, Title: {props.Title} has been updated, total count: {props.TotalCount}");
        });

        connection.Closed += OnConnectionClosed;
        Console.WriteLine("Connecting to the server");

        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connected to the server");
        }
        catch (HttpRequestException httpExc)
        {
            Console.WriteLine($"Http Error happened while connecting to the server: {httpExc.Message}");
            throw;
        }
        catch (HubException ex)
        {
            Console.WriteLine($"Hub Error happened while connecting to the server: {ex.Message}");
            throw;
        }
    }

    public async Task CloseAsync()
    {
        Console.WriteLine("Stopping connection");
        try
        {
            await connection.StopAsync();
            Console.WriteLine("Connection stopped");
        }
        catch (HubException ex)
        {
            Console.WriteLine($"Error happened while stopping connection to the server: {ex.Message}");
        }
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            if (message.StartsWith("subsvote", StringComparison.CurrentCultureIgnoreCase))
            {
                var voteId = message.Split(" ")[1];
                if (voteId != null)
                {
                    var result = await connection.InvokeAsync<bool>("SubscribeVote", user, voteId);
                    if (result)
                    {
                        Console.WriteLine($"Succesfully subscribed to vote {voteId}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to subscribe to vote {voteId}");
                    }
                    return;
                }
            }

            if (message.StartsWith("unsubsvote", StringComparison.CurrentCultureIgnoreCase))
            {
                var voteId = message.Split(" ")[1];
                if (voteId != null)
                {
                    var result = await connection.InvokeAsync<bool>("UnsubscribeVote", user, voteId);
                    if (result)
                    {
                        Console.WriteLine($"Succesfully unsubscribed from vote {voteId}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to unsubscribe from vote {voteId}");
                    }
                    return;
                }
            }

            await connection.SendAsync("send", new { Sender = user, Message = message });
        }
        catch (InvalidOperationException)
        {
            throw new ChatClientException("Error happened while sending message. Connection has been closed");
        }
    }

    private Task OnConnectionClosed(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"Connection was closed: {ex.Message}");
            Console.Clear();
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