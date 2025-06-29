using SignalRDemo.Server.Endpoints.Requests;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateVoteRequestExtensions
{
    public static Vote ToVote(this CreateVoteRequest request, User? creator = null)
    {
        var vote = Vote.Create(request.Title,
            request.Subjects,
            creator,
            request.Duration,
            request.MaximumCount);

        return vote;
    }
}