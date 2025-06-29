namespace SimpleVote.Server.Models;

public record struct VoteQueueItem(string VoteId, string SubjectId, string? UserId);