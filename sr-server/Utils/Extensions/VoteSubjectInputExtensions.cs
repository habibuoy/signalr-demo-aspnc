using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class VoteSubjectInputExtensions
{
    public static VoteInputDto ToDto(this VoteSubjectInput subjectInput)
    {
        return new()
        {
            Id = subjectInput.Id,
            VoteId = subjectInput.VoteSubject!.VoteId,
            VoteTitle = subjectInput.VoteSubject!.Vote!.Title,
            SubjectId = subjectInput.SubjectId,
            SubjectName = subjectInput.VoteSubject!.Name,
            InputTime = subjectInput.InputTime,
        };
    }
}