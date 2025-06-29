using SimpleVote.Server.Endpoints.Requests;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

public static class UpdateVoteRequestExtensions
{
    public static Vote ToVote(this UpdateVoteRequest request)
    {
        var vote = Vote.Create(request.Title,
            request.Subjects.Select(s => new VoteSubject()
            {
                Id = s.Id,
                Name = s.Name
            }).ToArray(),
            null,
            request.Duration,
            request.MaximumCount);

        return vote;
    }
}