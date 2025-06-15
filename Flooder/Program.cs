// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SignalRDemo.Flooder;
using SignalRDemo.Shared;
using static SignalRDemo.Shared.AppDefaults;
using static SignalRDemo.Shared.HttpHelper;
using static SignalRDemo.Shared.FileHelper;

#region Variables
var services = new ServiceCollection();
services.AddHttpClient();

var serviceProvider = services.BuildServiceProvider();

const float DelayBeforePostingVote = 1f; // in s
const float DelayBeforeFlood = 3f; // in s

const string CreateVoteUrl = $"{BaseVoteUrl}/";
const string Email = "creator@gmail.com";
const string Password = "Password123@";
const string VoteRecordsFilePath = "./vote-records.json";
const string VoteConfigFilePath = "./vote-config.json";

var floodTypePrompter = new InputPrompter("Choose (1/2):",
    "+++++++++++\n" +
    "1. New flood (Post a new flood and then target that)\n" +
    "2. Targeted flood (You input the vote id)\n" +
    "+++++++++++",
    ["1", "2"]);

var voteEndpointTypePrompter = new InputPrompter(
    "Choose (1/2): ",
    "Choose endpoint type:\n1.Normal vote\n2.Queue vote",
    ["1", "2"]);

var voteIdPrompter = new InputPrompter(
    "Input Vote Id: ",
    "Target Vote Id (Leave empty to let the app gets it)",
    inputEvaluator: null);

var floodCountPrompter = new InputPrompter("Enter count: ",
    "============\nPlease enter flood count in number\r",
    IsNumber);

var maxRetryCountPrompter = new InputPrompter("Max retry count: ", inputEvaluator: IsNumber);

var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
#endregion

#region Main
while (true)
{
    string? voteId = voteIdPrompter.Prompt();
    int endpointType = int.Parse(voteEndpointTypePrompter.Prompt());

    if (string.IsNullOrEmpty(voteId))
    {
        Console.WriteLine($"Waiting {DelayBeforePostingVote} seconds before posting a vote");

        await Task.Delay((int)(DelayBeforePostingVote * 1000));

        voteId = await PostVoteAsync(httpClientFactory.CreateClient(), null);
    }
    else
    {
        var httpClient = httpClientFactory.CreateClient();

        try
        {
            var loginResponse = await LoginCreatorAsync(httpClient);
            if (loginResponse.cookie == null)
            {
                Console.WriteLine($"Failed when checking vote: {loginResponse.message}");
                voteId = null;
            }
            else
            {
                var result = await SendHttpRequestAsync(httpClient,
                    $"{BaseVoteUrl}/{voteId}", HttpMethod.Get, cookie: loginResponse.cookie);
                if (!result.success)
                {
                    Console.WriteLine($"Failed when checking vote: {result.message}");
                    voteId = null;
                }
            }

        }
        finally
        {
            httpClient.Dispose();
        }
    }

    if (string.IsNullOrEmpty(voteId))
    {
        Console.WriteLine("Vote Id is not valid, restarting app");
        continue;
    }

    Console.WriteLine($"Vote Id: {voteId}");

    var fc = floodCountPrompter.Prompt();

    int maxRetryCount = int.Parse(maxRetryCountPrompter.Prompt());

    var floodTask = Task.Run(async () =>
    {
        Console.WriteLine($"Waiting {DelayBeforeFlood} seconds before flooding");

        await Task.Delay((int)(DelayBeforeFlood * 1000));

        Console.WriteLine("Beginning flooding");
        int floodCount = int.Parse(fc);
        var voteTasks = new Task[floodCount];

        Console.WriteLine("Using async flood");
        var floodSw = new Stopwatch();
        await Task.Delay(1000);
        floodSw.Start();

        using var semaphore = new SemaphoreSlim(100, 500);
        for (int i = 0; i < floodCount; i++)
        {
            var index = i;
            await semaphore.WaitAsync();
            voteTasks[i] = Task.Run(async () =>
            {
                try
                {
                    await FloodTaskAsync(index, voteId, endpointType, httpClientFactory, maxRetryCount);
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

        await Task.WhenAll(voteTasks);
        floodSw.Stop();

        Console.WriteLine($"All {floodCount} flood requests has been done in {floodSw.Elapsed:g} on Vote Id {voteId}");
        Console.WriteLine("=========================================================================================");
    });

    await floodTask;
    await Task.Delay(1000);

    Console.WriteLine("Press any key to begin again");
    Console.ReadKey();
}
#endregion

#region Functions
static async Task FloodTaskAsync(int index, string voteId, int endpointType,
    IHttpClientFactory? httpClientFactory = null, int voteMaxRetry = 1)
{
    // according to the official website,
    // we have to avoid using http client factory because we use cookies
    var httpClient = new HttpClient();

    try
    {
        var credentialBody = JsonContent.Create(new
        {
            Email = $"flooder{index}@gmail.com",
            Password = "Password123@",
            FirstName = "Flooder",
            LastName = index.ToString()
        });

        var loginResponse = await LoginAsync(httpClient, credentialBody);
        if (loginResponse.cookie == null)
        {
            if (loginResponse.statusCode == HttpStatusCode.NotFound)
            {
                var registerRespone = await RegisterAsync(httpClient, credentialBody);
                if (!registerRespone.success)
                {
                    Console.WriteLine($"Flood task {index} failed when registering: {registerRespone.message}");
                    return;
                }

                loginResponse = await LoginAsync(httpClient, credentialBody);
            }
        }

        if (loginResponse.cookie == null)
        {
            Console.WriteLine($"Flood task {index} failed when logging in: {loginResponse.message}");
            return;
        }

        var voteInfoResponse = await SendHttpRequestAsync(httpClient, $"{BaseVoteUrl}/{voteId}", HttpMethod.Get,
            cookie: loginResponse.cookie);

        if (!voteInfoResponse.success)
        {
            Console.WriteLine($"Flood task {index} failed when fetching vote info: {voteInfoResponse.message}");
            return;
        }

        if (voteInfoResponse.resultJson is not JsonNode result)
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

        bool isSuccessVote = false;
        int retryRemaining = voteMaxRetry + 1;
        string? message = null;

        while (!isSuccessVote && retryRemaining > 0)
        {
            string endpoint = endpointType == 1
                ? $"{BaseVoteUrl}/inputs?voteId={voteId}&subjectId={subjectId}"
                : $"{BaseVoteUrl}/inputs/queue?voteId={voteId}&subjectId={subjectId}";
            var voteSubjectResponse = await SendHttpRequestAsync(httpClient,
                endpoint,
                HttpMethod.Post,
                cookie: loginResponse.cookie);

            if (!voteSubjectResponse.success)
            {
                retryRemaining--;
                message = voteSubjectResponse.message;
            }
            else
            {
                isSuccessVote = true;
            }

            await Task.Delay(Random.Shared.Next(10, 100));
        }

        if (!isSuccessVote)
        {
            Console.WriteLine($"Flood task {index} failed when posting subject vote after retrying for {voteMaxRetry} time(s)\n{message}");
            return;
        }

        switch (endpointType)
        {
            default:
                Console.WriteLine($"Flood task {index} successfully voted subject {subjectId} on vote {voteId}");
                break;
            case 2:
                Console.WriteLine($"Flood task {index} successfully queued vote subject {subjectId} on vote {voteId}");
                break;
        }
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
                Console.WriteLine($"Error happened ({ex.GetType()}) on flood task {index} while doing the request: {ex.Message}, {ex}");
                break;
        }

        return;
    }
    finally
    {
        httpClient.Dispose();
    }
}

static bool IsNumber(string input)
{
    return int.TryParse(input, out _);
}

static async Task<string?> PostVoteAsync(HttpClient? httpClient, Cookie? cookie)
{
    Console.WriteLine("Begin posting a vote");

    try
    {
        httpClient ??= new HttpClient();

        Console.WriteLine("Logging in...");
        var loginResponse = await LoginCreatorAsync(httpClient);
        if (loginResponse.cookie == null)
        {
            Console.WriteLine($"Failed to log in: {loginResponse.message}");
            return null;
        }

        var loadRecord = LoadOrCreateFromJsonFile(VoteRecordsFilePath, 
            static () => new VoteRecords(), defaultJsonSerializerOptions);

        if (loadRecord.result == null)
        {
            Console.WriteLine($"Failed to load or create vote records: {loadRecord.message}");
            return null;
        }

        var loadConfig = LoadOrCreateFromJsonFile(VoteConfigFilePath, 
            static () => VoteConfig.Default, defaultJsonSerializerOptions);

        if (loadConfig.result == null)
        {
            Console.WriteLine($"Failed to load or create vote config: {loadConfig.message}");
            return null;
        }

        var records = loadRecord.result;
        var config = loadConfig.result;

        var subjects = new string[config!.SubjectCount];
        for (int i = 0; i < subjects.Length; i++)
        {
            subjects[i] = $"Subject {records!.LastSubjectIndex++ + 1}";
        }
        var votingContent = JsonContent.Create(new
        {
            Title = $"Test vote {records!.LastVoteIndex++}",
            Subjects = subjects,
            Duration = config.Duration
        });

        Console.WriteLine("Posting a vote...");
        var response = await SendHttpRequestAsync(httpClient, CreateVoteUrl, HttpMethod.Post, votingContent, cookie);

        if (!response.success)
        {
            Console.WriteLine($"Failed when posting vote: {response.message}");

            return null;
        }

        if (response.resultJson == null)
        {
            return null;
        }

        if (response.resultJson["id"] is not JsonNode voteId)
        {
            return null;
        }

        Console.WriteLine("Finished posting a vote");

        if (SaveToJsonFile(VoteRecordsFilePath, records, defaultJsonSerializerOptions)
            is { isSuccess: false } rec)
        {
            Console.WriteLine($"Failed when saving records file: {rec.message}");
        }

        if (SaveToJsonFile(VoteConfigFilePath, config, defaultJsonSerializerOptions)
            is { isSuccess: false } conf)
        {
            Console.WriteLine($"Failed when saving records file: {conf.message}");
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
    finally
    {
        
    }
}

static Task<(Cookie? cookie, HttpStatusCode statusCode, string? message)> LoginCreatorAsync(HttpClient? httpClient)
{
    HttpContent creatorLoginRequestBody = JsonContent.Create(new
    {
        Email = Email,
        Password = Password
    });

    return LoginAsync(httpClient, creatorLoginRequestBody);
}
#endregion