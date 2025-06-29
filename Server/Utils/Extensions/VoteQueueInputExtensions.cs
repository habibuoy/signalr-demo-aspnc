using SimpleVote.Server.Endpoints.Responses;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

public static class VoteQueueInputExtensions
{
    public static VoteQueueResponse ToResponse(this VoteQueueInput input)
    {
        return new VoteQueueResponse(input.VoteId, input.SubjectId,
            input.VoterId, input.InputTime, input.ProcessedTime,
            input.Status.ToString(), input.StatusDetail);
    }
}