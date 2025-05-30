using SignalRDemo.Server.Models;
using SignalRDemo.Server.Models.Dtos;
using SignalRDemo.Shared;

namespace SignalRDemo.Server.Utils.Extensions;

public static class VoteExtensions
{
    public static VoteDto ToDto(this Vote vote)
    {
        var subjects = new List<VoteSubjectDto>();
        int totalCount = 0;

        foreach (var subject in vote.Subjects)
        {
            int count = subject.Voters.Count;

            subjects.Add(new()
            {
                Id = subject.Id,
                Name = subject.Name,
                VoteCount = count
            });

            totalCount += count;
        }

        return new VoteDto()
        {
            Id = vote.Id,
            Title = vote.Title,
            Subjects = subjects,
            CreatedTime = vote.CreatedTime,
            ExpiredTime = vote.ExpiredTime.HasValue ? vote.ExpiredTime.Value : null,
            MaximumCount = vote.MaximumCount,
            CurrentTotalCount = totalCount,
            CreatorId = vote.CreatorId
        };
    }

    public static VoteCreatedProperties ToVoteCreatedProperties(this Vote vote)
    {
        return new VoteCreatedProperties()
        {
            Id = vote.Id,
            Title = vote.Title,
            CreatedTime = vote.CreatedTime,
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