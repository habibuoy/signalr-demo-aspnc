using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRDemo.Client;

const string Endpoint = "https://localhost:7000/watchvote";

string? user = string.Empty;
while (string.IsNullOrEmpty(user))
{
    Console.Write("Enter user: ");
    user = Console.ReadLine();
}

var client = new VoteClient(Endpoint, user);

try
{
    await client.InitializeAsync();
}
catch (HubException)
{
    Console.WriteLine("Failed to initialize chat client, quitting app");
    return;
}

string? message = string.Empty;

while (client.State == HubConnectionState.Connected)
{
    Console.Write("Write message: ");
    message = Console.ReadLine();

    if (string.IsNullOrEmpty(message))
    {
        continue;
    }

    if (message == "STOP")
    {
        try
        {
            await client.CloseAsync();
        }
        catch (HubException ex)
        {
            Console.WriteLine(ex);
        }

        break;
    }

    try
    {
        await client.SendMessageAsync(message);
    }
    catch (ChatClientException ex)
    {
        Console.WriteLine(ex);
        continue;
    }
}

Console.WriteLine("Quitting app");