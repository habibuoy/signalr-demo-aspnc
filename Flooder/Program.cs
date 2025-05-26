// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SignalRDemo.Flooder;
using SignalRDemo.Shared;

var services = new ServiceCollection();
services.AddHttpClient();

var serviceProvider = services.BuildServiceProvider();

const float DelayBeforePostingVote = 1f; // in s
const float DelayBeforeFlood = 3f; // in s

const string LoginUrl = "https://localhost:7000/login";
const string RegisterUrl = "https://localhost:7000/register";
const string BaseVoteUrl = "https://localhost:7000/vote";
const string CreateVoteUrl = $"{BaseVoteUrl}/create";
const string Email = "creator@gmail.com";
const string Password = "Password123@";
const string ServerResponseHeaderCookieKey = "Set-Cookie";
const string VoteRecordsFilePath = "./vote-records.json";
const string VoteConfigFilePath = "./vote-config.json";

var floodTypePrompter = new InputPrompter("Choose (1/2):",
    "+++++++++++\n" +
    "1. New flood (Post a new flood and then target that)\n" +
    "2. Targeted flood (You input the vote id)\n" +
    "+++++++++++",
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

while (true)
{
    string? voteId = voteIdPrompter.Prompt();

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
                    await FloodTaskAsync(index, voteId, httpClientFactory, maxRetryCount);
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

static async Task FloodTaskAsync(int index, string voteId,
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
            Password = "Password123@"
        });

        var loginResponse = await LoginAsync(httpClient, credentialBody);
        if (loginResponse.cookie == null)
        {
            if (loginResponse.statusCode == HttpStatusCode.NotFound)
            {
                var registerRespone = await SendHttpRequestAsync(httpClient, RegisterUrl, HttpMethod.Post, credentialBody);
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
            var voteSubjectResponse = await SendHttpRequestAsync(httpClient,
                $"{BaseVoteUrl}?voteId={voteId}&subjectId={subjectId}",
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

        VoteRecords? records = null;
        VoteConfig? config = null;
        
        var jsonOptions = new JsonSerializerOptions()
        {
            // prettify
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        try
        {
            using var voteRecordsFile = File.Open(VoteRecordsFilePath, FileMode.OpenOrCreate);
            records = JsonSerializer.Deserialize<VoteRecords>(voteRecordsFile, jsonOptions);
        }
        catch (JsonException)
        {
            records = new VoteRecords();
        }

        try
        {
            using var voteConfigFile = File.Open(VoteConfigFilePath, FileMode.OpenOrCreate);
            config = JsonSerializer.Deserialize<VoteConfig>(voteConfigFile, jsonOptions);
        }
        catch (JsonException)
        {
            config = VoteConfig.Default;
        }

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

        File.WriteAllText(VoteRecordsFilePath, JsonSerializer.Serialize(records, jsonOptions));
        File.WriteAllText(VoteConfigFilePath, JsonSerializer.Serialize(config, jsonOptions));

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

static Cookie GetCookie(string setCookieString)
{
    ArgumentException.ThrowIfNullOrEmpty(setCookieString);

    var splitted = setCookieString.Split("; ");
    if (splitted.Length == 0)
    {
        return null!;
    }

    var cookie = new Cookie();
    var nameValueSplit = splitted[0].Split("=");
    if (nameValueSplit.Length == 2)
    {
        cookie.Name = nameValueSplit[0];
        cookie.Value = nameValueSplit[1];
    }

    foreach (var piece in splitted)
    {
        if (piece.Contains("path"))
        {
            var splitPath = piece.Split("=");
            if (splitPath.Length == 2)
            {
                cookie.Path = splitPath[1];
            }
        }
        else if (piece.Contains("secure"))
        {
            cookie.Secure = true;
        }
        else if (piece.Contains("httponly"))
        {
            cookie.HttpOnly = true;
        }
    }

    return cookie;
}

static async Task<(Cookie? cookie, HttpStatusCode statusCode, string? message)> LoginAsync(HttpClient? httpClient, HttpContent? requestBody)
{
    httpClient ??= new HttpClient();

    var loginResponse = await SendHttpRequestAsync(httpClient, LoginUrl, HttpMethod.Post,
        requestBody);

    if (!loginResponse.success)
    {
        return (null, loginResponse.statusCode, loginResponse.message);
    }

    if (!loginResponse.headers.TryGetValues(ServerResponseHeaderCookieKey, out var cookies) || !cookies.Any())
    {
        return (null, loginResponse.statusCode, "Login success but no cookie returned");
    }

    return (GetCookie(cookies.Single()), loginResponse.statusCode, loginResponse.message);
}

static async Task<(bool success, HttpStatusCode statusCode, string? message, JsonNode? resultJson, HttpResponseHeaders headers)>
    SendHttpRequestAsync(HttpClient httpClient, string url, HttpMethod method,
        HttpContent? content = null, Cookie? cookie = null)
{
    var httpRequestMessage = new HttpRequestMessage()
    {
        RequestUri = new Uri(url),
        Method = method,
        Content = content,
    };

    if (cookie != null)
    {
        httpRequestMessage.Headers.Add("Cookie", cookie.ToString());
    }

    try
    {
        var response = await httpClient.SendAsync(httpRequestMessage);
        var body = await JsonNode.ParseAsync(response.Content.ReadAsStream());
        string? message = null;
        JsonNode? resultObj = null;

        if (body != null)
        {
            if (body["message"] is JsonNode messageJson)
            {
                message = messageJson.GetValue<string>();
            }

            if (body["result"] is JsonNode resultJson)
            {
                resultObj = resultJson;
            }
        }

        return (response.IsSuccessStatusCode, response.StatusCode, message, resultObj, response.Headers);
    }
    catch (Exception)
    {
        throw;
    }
}