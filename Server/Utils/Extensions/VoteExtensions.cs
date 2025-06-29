using SimpleVote.Server.Endpoints.Responses;
using SimpleVote.Server.Models;
using SimpleVote.Shared;

namespace SimpleVote.Server.Utils.Extensions;

public static class VoteExtensions
{
    public static VoteResponse ToResponse(this Vote vote)
    {
        var subjects = new List<VoteSubjectResponse>();
        int totalCount = 0;

        foreach (var subject in vote.Subjects)
        {
            int count = subject.Voters.Count;

            subjects.Add(new(subject.Id, subject.Name, count));

            totalCount += count;
        }

        return new VoteResponse(vote.Id, vote.Title, subjects, totalCount, vote.CreatedTime,
            vote.ExpiredTime.HasValue ? vote.ExpiredTime.Value : null, vote.MaximumCount, vote.CreatorId);
    }

    public static VoteCreatedProperties ToVoteCreatedProperties(this Vote vote)
    {
        return new VoteCreatedProperties()
        {
            Id = vote.Id,
            Title = vote.Title,
            CreatedTime = vote.CreatedTime,
            MaximumCount = vote.MaximumCount,
            ExpiredTime = vote.ExpiredTime,
            Subjects = vote.Subjects.Select(s => new VoteSubjectProperties()
            {
                Id = s.Id,
                Name = s.Name,
                VoteCount = s.Voters.Count
            }).ToList()
        };
    }

    public static VoteUpdatedProperties ToVoteUpdatedProperties(this Vote vote)
    {
        return new VoteUpdatedProperties()
        {
            Id = vote.Id,
            Title = vote.Title,
            TotalCount = vote.Subjects.Aggregate(0, (acc, c) =>
            {
                acc += c.Voters.Count;
                return acc;
            }),
            Subjects = vote.Subjects.Select(s => new VoteSubjectProperties()
            {
                Id = s.Id,
                Name = s.Name,
                VoteCount = s.Voters.Count
            }).ToList()
        };
    }
}