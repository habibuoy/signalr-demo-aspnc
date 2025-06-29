namespace SimpleVote.Server.Endpoints.Responses;

public record VoteQueueResponse(string VoteId, string SubjectId, string? VoterId,
    DateTime InputTime, DateTime? ProcessedTime,
    string Status, string StatusDetail);