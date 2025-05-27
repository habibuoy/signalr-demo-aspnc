using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using static SignalRDemo.Shared.AppDefaults;

namespace SignalRDemo.Shared;

public static class HttpHelper
{
    public static Cookie GetCookie(string setCookieString)
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

    public static async Task<(Cookie? cookie, HttpStatusCode statusCode, string? message)> LoginAsync(HttpClient? httpClient, HttpContent? requestBody)
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

    public static async Task<(bool success, HttpStatusCode statusCode, string? message)> RegisterAsync(HttpClient? httpClient, HttpContent? requestBody)
    {
        httpClient ??= new HttpClient();

        var registerResponse = await SendHttpRequestAsync(httpClient, RegisterUrl, HttpMethod.Post,
            requestBody);

        return (registerResponse.success, registerResponse.statusCode, registerResponse.message);
    }

    public static async Task<(bool success, HttpStatusCode statusCode, string? message)> LogoutAsync(HttpClient? httpClient, Cookie cookie)
    {
        httpClient ??= new HttpClient();

        var logoutResponse = await SendHttpRequestAsync(httpClient, LogoutUrl, HttpMethod.Get,
            cookie: cookie);

        return (logoutResponse.success, logoutResponse.statusCode, logoutResponse.message);
    }

    public static async Task<(bool success, HttpStatusCode statusCode, string? message, JsonNode? resultJson, HttpResponseHeaders headers)>
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
}