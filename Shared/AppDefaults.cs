namespace SimpleVote.Shared;

public static class AppDefaults
{
    public const string BaseUrl = "https://localhost:7000";
    public const string LoginUrl = $"{BaseUrl}/login";
    public const string LogoutUrl = $"{BaseUrl}/logout";
    public const string RegisterUrl = $"{BaseUrl}/register";
    public const string BaseVoteUrl = $"{BaseUrl}/votes";
    public const string ServerResponseHeaderCookieKey = "Set-Cookie";
}