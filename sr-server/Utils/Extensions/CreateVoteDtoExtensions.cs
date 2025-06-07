using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateVoteDtoExtensions
{
    public static Vote ToVote(this CreateVoteDto dto, User? creator = null)
    {
        var vote = Vote.Create(dto.Title, dto.Subjects, creator,
            dto.Duration, dto.MaximumCount);

        return vote;
    }
}