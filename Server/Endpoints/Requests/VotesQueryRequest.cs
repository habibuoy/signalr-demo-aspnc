namespace SimpleVote.Server.Endpoints.Requests;

public record VotesQueryRequest(int Page, int Count, string? SortBy, string? SortOrder, string? Search);