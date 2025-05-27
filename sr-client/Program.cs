using Microsoft.AspNetCore.SignalR.Client;
using SignalRDemo.Client;
using static SignalRDemo.Shared.HttpHelper;
using static SignalRDemo.Shared.FileHelper;

const string Endpoint = "https://localhost:7000/watchvote";
const string ClientConfigFilePath = "./client-config.json";

var loadResult = LoadOrCreateFromJsonFile(ClientConfigFilePath,
    static () => new ClientConfig(), defaultJsonSerializerOptions);

Console.WriteLine("Loading config file");

if (loadResult.result is not { } config)
{
    Console.WriteLine($"Failed when loading or creating client config file: {loadResult.message}\nQuitting app.");
    return;
}

Console.WriteLine("Config file loaded");

var client = new VoteClient(Endpoint, config.UserInfo);

Console.WriteLine($"Initializing client");
try
{
    await client.InitializeAsync();
}
catch (VoteClientException ex)
{
    Console.WriteLine($"Failed to initialize vote client, quitting app: {ex}");
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
        catch (VoteClientException ex)
        {
            Console.WriteLine(ex);
        }
        break;
    }

    if (message == "logout")
    {
        try
        {
            using var httpClient = new HttpClient();
            var logoutResponse = await LogoutAsync(httpClient, client.Cookie!);
            if (!logoutResponse.success)
            {
                Console.WriteLine($"Failed logout: {logoutResponse.message}");
                continue;
            }

            Console.WriteLine($"Succesfully logged out");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error happened while logging out: {ex}");
        }
        continue;
    }

    try
    {
        await client.SendMessageAsync(message);
    }
    catch (VoteClientException ex)
    {
        Console.WriteLine(ex);
        continue;
    }
}

Console.WriteLine("Quitting app");