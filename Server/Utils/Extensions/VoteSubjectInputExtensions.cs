using SimpleVote.Server.Endpoints.Responses;
using SimpleVote.Server.Models;

namespace SimpleVote.Server.Utils.Extensions;

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