// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SignalRDemo.Shared;

var services = new ServiceCollection();
services.AddHttpClient();

var serviceProvider = services.BuildServiceProvider();

const float DelayBeforePostingVote = 1f; // in s
const float DelayBeforeFlood = 3f; // in s
Console.WriteLine($"Waiting {DelayBeforePostingVote} seconds before posting a vote");

await Task.Delay((int) (DelayBeforePostingVote * 1000));

var voteId = await PostVoteAsync(null);

if (voteId == null)
{
    Console.WriteLine("VoteId null, cancelling flood");
    return;
}

Console.WriteLine($"Vote Id: {voteId}");

var floodCountPrompter = new InputPrompter("Enter count: ",
    "============\nPlease enter flood count in number\r",
    static (input) => int.TryParse(input, out _));
var fc = floodCountPrompter.Prompt();

var floodTask = Task.Run(async () =>
{
    Console.WriteLine($"Waiting {DelayBeforeFlood} seconds before flooding");

    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    await Task.Delay((int) (DelayBeforeFlood * 1000));

    Console.WriteLine("Beginning flooding");
    int floodCount = int.Parse(fc);
    var voteTasks = new Task[floodCount];

    Console.WriteLine("Using async flood");
    var floodSw = new Stopwatch();
    await Task.Delay(1000);
    floodSw.Start();

    var semaphore = new SemaphoreSlim(100, 500);
    for (int i = 0; i < floodCount; i++)
    {
        var index = i;
        await semaphore.WaitAsync();
        voteTasks[i] = Task.Run(async () =>
        {
            try
            {
                await FloodTaskAsync(index, voteId, httpClientFactory);
            }
            finally
            {
                semaphore.Release();
            }
        });
    }

    await Task.WhenAll(voteTasks);
    floodSw.Stop();

    Console.WriteLine($"All {floodCount} flood has been ran in {floodSw.Elapsed:g}");
});

await floodTask;

static async Task FloodTaskAsync(int index, string voteId,
    IHttpClientFactory httpClientFactory = null)
{
    var httpClient = httpClientFactory != null ? httpClientFactory.CreateClient() : new HttpClient();

    try
    {
        var voteInfoResponse = await httpClient.GetAsync($"https://localhost:7000/vote/{voteId}");

        JsonNode? voteInfoBody = await JsonNode.ParseAsync(voteInfoResponse.Content.ReadAsStream());

        if (!voteInfoResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Flood task {index} failed when fetching vote info");
            if (voteInfoBody != null && voteInfoBody["message"] is JsonNode message)
            {
                Console.WriteLine($"Message: {message.GetValue<string>()}");
            }
            return;
        }

        if (voteInfoBody == null
            || voteInfoBody["result"] is not JsonNode result)
        {
            return;
        }

        if (result["subjects"] is not JsonNode subjects)
        {
            return;
        }

        var subjectsId = subjects.AsArray();

        var randIndex = Random.Shared.Next(0, subjectsId.Count);
        var subject = subjectsId[randIndex];

        if (subject!["id"] is not JsonNode id)
        {
            return;
        }

        var subjectId = id.GetValue<int>();

        var voteSubjectResponse = await httpClient.PostAsync($"https://localhost:7000/vote?voteId={voteId}&subjectId={subjectId}", null);
        var voteSubjectBody = await JsonNode.ParseAsync(voteSubjectResponse.Content.ReadAsStream());

        if (!voteSubjectResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"Flood task {index} failed when posting subject vote");
            if (voteSubjectBody != null && voteSubjectBody["message"] is JsonNode message)
            {
                Console.WriteLine($"Message: {message.GetValue<string>()}");
            }
            return;
        }

        Console.WriteLine($"Flood task {index} successfully voted subject {subjectId} on vote {voteId}");
    }
    catch (Exception ex)
    {
        switch (ex)
        {
            case HttpRequestException:
                Console.WriteLine($"Http error happened on flood task {index} while doing the request: {ex.Message}");
                break;
            case InvalidOperationException:
                Console.WriteLine($"Operation Error happened on flood task {index} while doing the request: {ex.Message}");
                break;
            case JsonException:
                Console.WriteLine($"Json Error happened on flood task {index} while doing the request: {ex.Message}");
                break;
            default:
                Console.WriteLine($"Error happened on flood task {index} while doing the request: {ex.Message}");
                break;
        }

        return;
    }
    finally
    {
        httpClient.Dispose();
    }
}

static async Task<string?> PostVoteAsync(int? voteDuration = 60)
{
    Console.WriteLine("Begin posting a vote");

    try
    {
        var httpClient = new HttpClient();
        var httpContent = JsonContent.Create(new
        {
            Title = "Test vote",
            Subjects = new string[] { "Subject 1", "Subject 2" },
            Duration = voteDuration
        });

        Console.WriteLine("Posting a vote...");
        var response = await httpClient.PostAsync("https://localhost:7000/vote/create", httpContent);
        Console.WriteLine("Finished posting a vote");

        var body = await JsonNode.ParseAsync(response.Content.ReadAsStream());

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Posting vote response is not success: {response.StatusCode}");
            if (body != null && body["message"] is JsonNode message)
            {
                Console.WriteLine($"Message: {message.GetValue<string>()}");
            }

            return null;
        }

        if (body == null
            || body["result"] is not JsonNode result)
        {
            return null;
        }

        if (result["id"] is not JsonNode voteId)
        {
            return null;
        }

        return voteId.GetValue<string>();
    }
    catch (Exception ex)
    {
        switch (ex)
        {
            case HttpRequestException:
                Console.WriteLine($"Http error happened while doing the request: {ex.Message}");
                break;
            case InvalidOperationException:
                Console.WriteLine($"Operation Error happened while doing the request: {ex.Message}");
                break;
            case JsonException:
                Console.WriteLine($"Json Error happened while doing the request: {ex.Message}");
                break;
            default:
                Console.WriteLine($"Error happened while doing the request: {ex.Message}");
                break;
        }

        return null;
    }
}