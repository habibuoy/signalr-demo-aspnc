namespace SignalRDemo.Server.Endpoints.Requests;

public record VotesQueryRequest(int? Count, string? SortBy, string? SortOrder, string? Search);