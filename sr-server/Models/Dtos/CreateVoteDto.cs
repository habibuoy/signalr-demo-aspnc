using System.Text.Json;

namespace SignalRDemo.Server.Models.Dtos;

public class CreateVoteDto
{
    public required string Title { get; set; } = string.Empty;
    public required string[] Subjects { get; set; } = [];
    public int? Duration { get; set; }
    public int? MaximumCount { get; set; }

    public bool IsValid()
    {
        return Title.Length > 3
            && Subjects.Length > 1;
    }

    public static async ValueTask<CreateVoteDto?> BindAsync(HttpContext httpContext)
    {
        try
        {
            var dto = await httpContext.Request.ReadFromJsonAsync<CreateVoteDto>();
            return dto;
        }
        catch (JsonException ex)
        {
            httpContext.RequestServices.GetRequiredService<ILogger<Program>>()
                .LogInformation("Failed to parse CreateVoteDto: {msg}", ex.Message);
            return null;
        }
    }
}

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