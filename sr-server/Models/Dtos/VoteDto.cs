namespace SignalRDemo.Server.Models.Dtos;

public class VoteDto
{
    public required string Id { get; set; }
    public required string Title { get; set; } = string.Empty;
    public required IEnumerable<VoteSubjectDto> Subjects { get; set; } = [];
    public DateTime CreatedTime { get; set; }
    public DateTime? ExpiredTime { get; set; }
    public int? MaximumCount { get; set; }
    public int CurrentTotalCount { get; set; }
    public string? CreatorId { get; set; }
}