using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRDemo.Shared;

namespace SignalRDemo.Client;

public class ChatClient
{
    private readonly string endpoint;
    private readonly string user;

    private HubConnection connection;

    public HubConnectionState State => connection.State;

    public event Action ConnectionClosed;

    public ChatClient(string endpoint, string user)
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

        connection.Closed += OnConnectionClosed;
        Console.WriteLine("Connecting to the server");
        
        try
        {
            await connection.StartAsync();
            Console.WriteLine("Connected to the server");
        }
        catch (HubException ex)
        {
            Console.WriteLine($"Error happened while connecting to the server: {ex.Message}");
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
        }

        if (connection != null)
        {
            connection.Closed -= OnConnectionClosed;
        }

        ConnectionClosed?.Invoke();
        return Task.CompletedTask;
    }
}

public class ChatClientException : Exception
{
    public ChatClientException(string? message) 
        : base(message) { }

    public ChatClientException(string? message, Exception? innerException) 
        : base(message, innerException) { }
}