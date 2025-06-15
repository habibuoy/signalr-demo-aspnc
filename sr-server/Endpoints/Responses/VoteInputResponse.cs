namespace SignalRDemo.Server.Endpoints.Responses;

public record VoteInputResponse(int Id, int SubjectId, string SubjectName, string VoteId,
    string VoteTitle, DateTime? InputTime);