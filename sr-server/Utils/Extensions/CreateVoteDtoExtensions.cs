using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;

namespace SignalRDemo.Server.Utils.Extensions;

public static class CreateVoteDtoExtensions
{
    public static Vote ToVote(this CreateVoteDto dto, string? creatorId = null)
    {
        var vote = new Vote(dto.Title, dto.Subjects, maximumCount: dto.MaximumCount);
        if (dto.Duration != null)
        {
            vote.ExpiredTime = vote.CreatedTime.AddSeconds(dto.Duration.Value);
        }
        vote.CreatorId = creatorId;

        return vote;
    }
}