namespace SignalRDemo.Server.Endpoints.Responses;

public record VoteResponse(string Id, string Title, IEnumerable<VoteSubjectResponse> Subjects, int CurrentTotalCount,
    DateTime CreatedTime, DateTime? ExpiredTime, int? MaximumCount, string? CreatorId);