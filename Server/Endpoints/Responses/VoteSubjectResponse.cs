namespace SimpleVote.Server.Endpoints.Responses;

public record VoteSubjectResponse(int Id, string Name, int VoteCount);