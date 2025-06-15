using SignalRDemo.Server.Endpoints.Responses;
using SignalRDemo.Server.Models;

namespace SignalRDemo.Server.Utils.Extensions;

public static class VoteSubjectInputExtensions
{
    public static VoteInputResponse ToResponse(this VoteSubjectInput subjectInput)
    {
        return new(subjectInput.Id,
            subjectInput.SubjectId,
            subjectInput.VoteSubject!.Name,
            subjectInput.VoteSubject!.VoteId,
            subjectInput.VoteSubject!.Vote!.Title,
            subjectInput.InputTime);
    }
}