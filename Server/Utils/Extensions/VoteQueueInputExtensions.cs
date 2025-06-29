using SignalRDemo.Server.Endpoints.Responses;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class VoteQueueInputExtensions
{
    public static VoteQueueResponse ToResponse(this VoteQueueInput input)
    {
        return new VoteQueueResponse(input.VoteId, input.SubjectId,
            input.VoterId, input.InputTime, input.ProcessedTime,
            input.Status.ToString(), input.StatusDetail);
    }
}